using BPME.BPM.Host.Core.Configuration;
using BPME.BPM.Host.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BPME.BPM.Host.Core.DataBus
{
    /// <summary>
    /// Фабрика слушателей RabbitMQ.
    /// Создаёт отдельный RabbitMqListener для каждой очереди из конфигурации.
    ///
    /// Конфигурация в appsettings.json:
    /// "RabbitMQ": {
    ///     "Queues": [
    ///         { "QueueName": "bpm.process.start", "ProcessPublicId": null },
    ///         { "QueueName": "integration.orders", "ProcessPublicId": "order-workflow" }
    ///     ]
    /// }
    /// </summary>
    public class RabbitMqListenerFactory : IHostedService, IDisposable
    {
        private readonly IBPMState _bpmState;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqListenerFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly List<RabbitMqListener> _listeners = new();
        private bool _disposed;

        /// <summary>
        /// Создаёт фабрику слушателей
        /// </summary>
        public RabbitMqListenerFactory(
            IBPMState bpmState,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqListenerFactory> logger,
            ILoggerFactory loggerFactory)
        {
            _bpmState = bpmState;
            _options = options.Value;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Количество активных слушателей
        /// </summary>
        public int ActiveListenersCount => _listeners.Count(l => l.IsConnected);

        /// <summary>
        /// Список имён очередей, которые слушаются
        /// </summary>
        public IReadOnlyList<string> ListeningQueues => _listeners.Select(l => l.QueueName).ToList();

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("RabbitMQ слушатели отключены в конфигурации");
                return;
            }

            if (_options.Queues.Count == 0)
            {
                _logger.LogWarning("Не настроены очереди для прослушивания в RabbitMQ:Queues");
                return;
            }

            _logger.LogInformation(
                "=== RabbitMQ Listener Factory: запуск {Count} слушателей ===",
                _options.Queues.Count);

            foreach (var queueConfig in _options.Queues)
            {
                if (!queueConfig.Enabled)
                {
                    _logger.LogDebug("Очередь {Queue} отключена, пропускаем", queueConfig.QueueName);
                    continue;
                }

                try
                {
                    var listener = CreateListener(queueConfig);
                    _listeners.Add(listener);

                    await listener.StartListeningAsync(cancellationToken);

                    _logger.LogInformation(
                        "✓ Слушатель запущен: {Queue} → {Process}",
                        queueConfig.QueueName,
                        queueConfig.ProcessPublicId ?? "(из сообщения)");
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "✗ Не удалось запустить слушатель для {Queue}",
                        queueConfig.QueueName);
                }
            }

            _logger.LogInformation(
                "=== RabbitMQ Listener Factory: запущено {Active}/{Total} слушателей ===",
                _listeners.Count(l => l.IsConnected),
                _options.Queues.Count(q => q.Enabled));
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Остановка всех RabbitMQ слушателей...");

            var stopTasks = _listeners.Select(l => l.StopListeningAsync(cancellationToken));
            await Task.WhenAll(stopTasks);

            _logger.LogInformation("Все RabbitMQ слушатели остановлены");
        }

        private RabbitMqListener CreateListener(QueueListenerConfig queueConfig)
        {
            var logger = _loggerFactory.CreateLogger<RabbitMqListener>();

            return new RabbitMqListener(
                _bpmState,
                _options,
                queueConfig,
                logger);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;

            foreach (var listener in _listeners)
            {
                listener.Dispose();
            }

            _listeners.Clear();
            _disposed = true;
        }
    }
}
