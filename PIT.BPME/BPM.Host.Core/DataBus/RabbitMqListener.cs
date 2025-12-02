using BPME.BPM.Host.Core.Configuration;
using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BPME.BPM.Host.Core.DataBus
{
    /// <summary>
    /// Слушатель одной очереди RabbitMQ.
    /// Создаётся фабрикой RabbitMqListenerFactory для каждой очереди из конфигурации.
    /// </summary>
    public class RabbitMqListener : IDataBusListener, IDisposable
    {
        private readonly IBPMState _bpmState;
        private readonly RabbitMqOptions _connectionOptions;
        private readonly QueueListenerConfig _queueConfig;
        private readonly ILogger _logger;

        private IConnection? _connection;
        private IChannel? _channel;
        private bool _disposed;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Создаёт экземпляр слушателя для одной очереди
        /// </summary>
        public RabbitMqListener(
            IBPMState bpmState,
            RabbitMqOptions connectionOptions,
            QueueListenerConfig queueConfig,
            ILogger logger)
        {
            _bpmState = bpmState;
            _connectionOptions = connectionOptions;
            _queueConfig = queueConfig;
            _logger = logger;
        }

        /// <summary>
        /// Имя очереди, которую слушает этот listener
        /// </summary>
        public string QueueName => _queueConfig.QueueName;

        /// <inheritdoc />
        public bool IsConnected => _connection?.IsOpen ?? false;

        /// <inheritdoc />
        public async Task StartListeningAsync(CancellationToken cancellationToken = default)
        {
            if (!_queueConfig.Enabled)
            {
                _logger.LogInformation(
                    "Слушатель очереди {Queue} отключён в конфигурации",
                    _queueConfig.QueueName);
                return;
            }

            _logger.LogInformation(
                "Подключение к очереди {Queue} ({Description})",
                _queueConfig.QueueName,
                _queueConfig.Description ?? "без описания");

            try
            {
                await ConnectWithRetryAsync(cancellationToken);
                await SetupConsumerAsync(cancellationToken);

                _logger.LogInformation(
                    "Слушатель запущен: {Queue} → {ProcessId}",
                    _queueConfig.QueueName,
                    _queueConfig.ProcessPublicId ?? "из сообщения");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка подключения к очереди {Queue}", _queueConfig.QueueName);
                throw;
            }
        }

        /// <inheritdoc />
        public Task StopListeningAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Остановка слушателя очереди {Queue}...", _queueConfig.QueueName);
            Dispose();
            return Task.CompletedTask;
        }

        private async Task ConnectWithRetryAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _connectionOptions.Host,
                Port = _connectionOptions.Port,
                UserName = _connectionOptions.Username,
                Password = _connectionOptions.Password,
                VirtualHost = _connectionOptions.VirtualHost
            };

            for (int attempt = 1; attempt <= _connectionOptions.RetryCount; attempt++)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync(cancellationToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                    await _channel.QueueDeclareAsync(
                        queue: _queueConfig.QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: cancellationToken);

                    _logger.LogDebug(
                        "Подключено к {Queue} (попытка {Attempt})",
                        _queueConfig.QueueName, attempt);
                    return;
                }
                catch (Exception ex) when (attempt < _connectionOptions.RetryCount)
                {
                    _logger.LogWarning(
                        ex,
                        "Не удалось подключиться к {Queue} (попытка {Attempt}/{Max})",
                        _queueConfig.QueueName, attempt, _connectionOptions.RetryCount);

                    await Task.Delay(
                        TimeSpan.FromSeconds(_connectionOptions.RetryDelaySeconds),
                        cancellationToken);
                }
            }
        }

        private async Task SetupConsumerAsync(CancellationToken cancellationToken)
        {
            if (_channel == null) return;

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    await ProcessMessageAsync(ea);
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки сообщения из {Queue}", _queueConfig.QueueName);
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _queueConfig.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);
        }

        private Task ProcessMessageAsync(BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            _logger.LogDebug("[{Queue}] Получено: {Json}", _queueConfig.QueueName, json);

            var request = JsonSerializer.Deserialize<StartRequest>(json, JsonOptions);

            if (request == null)
            {
                _logger.LogWarning("[{Queue}] Не удалось десериализовать: {Json}",
                    _queueConfig.QueueName, json);
                return Task.CompletedTask;
            }

            // Если в конфиге очереди указан ProcessPublicId - используем его
            if (!string.IsNullOrEmpty(_queueConfig.ProcessPublicId))
            {
                request.ProcessPublicId = _queueConfig.ProcessPublicId;
            }

            if (string.IsNullOrEmpty(request.ProcessPublicId))
            {
                _logger.LogWarning("[{Queue}] Сообщение без ProcessPublicId: {Json}",
                    _queueConfig.QueueName, json);
                return Task.CompletedTask;
            }

            if (string.IsNullOrEmpty(request.CorrelationId))
            {
                request.CorrelationId = Guid.NewGuid().ToString();
            }

            _logger.LogInformation(
                "[{Queue}] → EnqueueRequest: {ProcessPublicId}, CorrelationId: {CorrelationId}",
                _queueConfig.QueueName, request.ProcessPublicId, request.CorrelationId);

            _bpmState.EnqueueRequest(request);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;

            _channel?.Dispose();
            _connection?.Dispose();

            _disposed = true;
            _logger.LogDebug("Слушатель {Queue} остановлен", _queueConfig.QueueName);
        }
    }
}
