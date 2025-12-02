using BPME.BPM.Host.Core.Configuration;
using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BPME.BPM.Host.Core.DataBus
{
    /// <summary>
    /// Слушатель RabbitMQ для получения запросов на запуск процессов.
    ///
    /// Схема работы:
    /// 1. Подключается к RabbitMQ при старте приложения
    /// 2. Слушает очередь bpm.process.start
    /// 3. При получении сообщения — парсит JSON в StartRequest
    /// 4. Вызывает IBPMState.EnqueueRequest()
    ///
    /// Формат сообщения (JSON):
    /// {
    ///     "processPublicId": "order-process",
    ///     "correlationId": "unique-id-123",
    ///     "inputData": { "orderId": 123 }
    /// }
    ///
    /// На схеме это IDataBusListener → RabbitMQ.
    /// </summary>
    public class RabbitMqListener : IDataBusListener, IHostedService, IDisposable
    {
        private readonly IBPMState _bpmState;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqListener> _logger;

        private IConnection? _connection;
        private IChannel? _channel;
        private bool _disposed;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public RabbitMqListener(
            IBPMState bpmState,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqListener> logger)
        {
            _bpmState = bpmState;
            _options = options.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public bool IsConnected => _connection?.IsOpen ?? false;

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("RabbitMQ listener отключён в конфигурации");
                return;
            }

            await StartListeningAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopListeningAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task StartListeningAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Подключение к RabbitMQ: {Host}:{Port}",
                _options.Host, _options.Port);

            try
            {
                await ConnectWithRetryAsync(cancellationToken);
                await SetupConsumerAsync(cancellationToken);

                _logger.LogInformation(
                    "RabbitMQ listener запущен. Очередь: {Queue}",
                    _options.QueueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка подключения к RabbitMQ");
                throw;
            }
        }

        /// <inheritdoc />
        public Task StopListeningAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Остановка RabbitMQ listener...");

            Dispose();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Подключение с повторными попытками
        /// </summary>
        private async Task ConnectWithRetryAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            for (int attempt = 1; attempt <= _options.RetryCount; attempt++)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync(cancellationToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                    // Объявляем очередь (создастся, если не существует)
                    await _channel.QueueDeclareAsync(
                        queue: _options.QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: cancellationToken);

                    _logger.LogInformation("Подключено к RabbitMQ (попытка {Attempt})", attempt);
                    return;
                }
                catch (Exception ex) when (attempt < _options.RetryCount)
                {
                    _logger.LogWarning(
                        ex,
                        "Не удалось подключиться к RabbitMQ (попытка {Attempt}/{Max}). Повтор через {Delay} сек.",
                        attempt, _options.RetryCount, _options.RetryDelaySeconds);

                    await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Настройка consumer'а
        /// </summary>
        private async Task SetupConsumerAsync(CancellationToken cancellationToken)
        {
            if (_channel == null) return;

            // Prefetch = 1: обрабатываем по одному сообщению
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    await ProcessMessageAsync(ea);

                    // Подтверждаем обработку
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки сообщения");

                    // Отклоняем без requeue (чтобы не зациклиться)
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _options.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Обработка входящего сообщения
        /// </summary>
        private Task ProcessMessageAsync(BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            _logger.LogDebug("Получено сообщение: {Json}", json);

            var request = JsonSerializer.Deserialize<StartRequest>(json, JsonOptions);

            if (request == null)
            {
                _logger.LogWarning("Не удалось десериализовать сообщение: {Json}", json);
                return Task.CompletedTask;
            }

            if (string.IsNullOrEmpty(request.ProcessPublicId))
            {
                _logger.LogWarning("Сообщение без ProcessPublicId: {Json}", json);
                return Task.CompletedTask;
            }

            // Генерируем CorrelationId, если не передан
            if (string.IsNullOrEmpty(request.CorrelationId))
            {
                request.CorrelationId = Guid.NewGuid().ToString();
            }

            _logger.LogInformation(
                "RabbitMQ → EnqueueRequest: {ProcessPublicId}, CorrelationId: {CorrelationId}",
                request.ProcessPublicId, request.CorrelationId);

            // Добавляем в очередь на выполнение
            _bpmState.EnqueueRequest(request);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _channel?.Dispose();
            _connection?.Dispose();

            _disposed = true;

            _logger.LogInformation("RabbitMQ listener остановлен");
        }
    }
}
