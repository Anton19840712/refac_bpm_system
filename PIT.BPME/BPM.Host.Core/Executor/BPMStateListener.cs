using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;

namespace BPME.BPM.Host.Core.Executor
{
    /// <summary>
    /// Фоновый сервис, слушающий состояние BPM и запускающий процессы.
    ///
    /// На схеме это IBPMStateListener (HostedService).
    ///
    /// Как работает:
    /// 1. При старте подписывается на IBPMState.PendingRequests.CollectionChanged
    /// 2. Когда приходит новый StartRequest — создаёт scope
    /// 3. Загружает ProcessConfig (пока заглушка)
    /// 4. Создаёт ProcessExecutorService и запускает выполнение
    /// 5. После выполнения — удаляет запрос из очереди
    /// </summary>
    public class BPMStateListener : IHostedService
    {
        private readonly IBPMState _bpmState;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BPMStateListener> _logger;

        /// <summary>
        /// Создаёт слушатель состояния BPM
        /// </summary>
        public BPMStateListener(
            IBPMState bpmState,
            IServiceScopeFactory scopeFactory,
            ILogger<BPMStateListener> logger)
        {
            _bpmState = bpmState;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BPMStateListener: запущен");

            // Подписываемся на новые запросы
            _bpmState.PendingRequests.CollectionChanged += OnRequestReceived;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BPMStateListener: остановлен");

            // Отписываемся
            _bpmState.PendingRequests.CollectionChanged -= OnRequestReceived;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Обработчик новых запросов на выполнение процессов.
        /// </summary>
        private void OnRequestReceived(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null)
            {
                return;
            }

            foreach (var item in e.NewItems)
            {
                if (item is StartRequest request)
                {
                    _logger.LogInformation(
                        "Получен запрос на выполнение процесса {ProcessId}, CorrelationId: {CorrelationId}",
                        request.ProcessPublicId,
                        request.CorrelationId);

                    // Запускаем выполнение в отдельной задаче
                    _ = Task.Run(() => ExecuteProcessAsync(request));
                }
            }
        }

        /// <summary>
        /// Выполняет процесс по запросу.
        /// </summary>
        private async Task ExecuteProcessAsync(StartRequest request)
        {
            // Создаём scope для Scoped-сервисов
            using var scope = _scopeFactory.CreateScope();

            var instanceRepo = scope.ServiceProvider
                .GetRequiredService<IProcessInstanceRepository>();

            // Создаём запись о запуске процесса
            var instanceId = Guid.NewGuid().ToString();
            var instance = new ProcessInstanceDto
            {
                InstanceId = instanceId,
                CorrelationId = request.CorrelationId,
                ProcessPublicId = request.ProcessPublicId,
                Status = "Pending",
                InputArguments = request.InputArguments,
                Source = request.Source,
                CreatedAt = request.CreatedAt
            };

            try
            {
                await instanceRepo.CreateAsync(instance);

                _logger.LogDebug(
                    "Процесс {ProcessId}: начало обработки запроса, InstanceId: {InstanceId}",
                    request.ProcessPublicId, instanceId);

                // 1. Загружаем конфигурацию процесса через сервис
                var configService = scope.ServiceProvider
                    .GetRequiredService<IConfigureService<ProcessConfig>>();

                var processConfig = await configService.GetByPublicIdAsync(request.ProcessPublicId);

                if (processConfig == null)
                {
                    instance.Status = "Failed";
                    instance.ErrorMessage = "Конфигурация не найдена";
                    instance.CompletedAt = DateTime.UtcNow;
                    await instanceRepo.UpdateAsync(instance);

                    _logger.LogError(
                        "Процесс {ProcessId}: конфигурация не найдена",
                        request.ProcessPublicId);
                    return;
                }

                if (!processConfig.IsActive)
                {
                    instance.Status = "Failed";
                    instance.ErrorMessage = "Конфигурация неактивна";
                    instance.CompletedAt = DateTime.UtcNow;
                    await instanceRepo.UpdateAsync(instance);

                    _logger.LogWarning(
                        "Процесс {ProcessId}: конфигурация неактивна",
                        request.ProcessPublicId);
                    return;
                }

                // Отмечаем начало выполнения
                instance.Status = "Running";
                instance.StartedAt = DateTime.UtcNow;
                await instanceRepo.UpdateAsync(instance);

                // 2. Создаём исполнитель процесса
                var executor = scope.ServiceProvider
                    .GetRequiredService<ProcessExecutorService>();

                executor.InputValue = request.InputArguments;

                // 3. Выполняем процесс
                await executor.ExecuteAsync(processConfig);

                // 4. Сохраняем результат
                instance.Status = "Completed";
                instance.OutputResult = executor.OutputValue;
                instance.CompletedAt = DateTime.UtcNow;
                await instanceRepo.UpdateAsync(instance);

                _logger.LogInformation(
                    "Процесс {ProcessId}: выполнение завершено успешно, InstanceId: {InstanceId}",
                    request.ProcessPublicId, instanceId);
            }
            catch (Exception ex)
            {
                instance.Status = "Failed";
                instance.ErrorMessage = ex.Message;
                instance.CompletedAt = DateTime.UtcNow;

                try
                {
                    await instanceRepo.UpdateAsync(instance);
                }
                catch
                {
                    // Если не удалось сохранить ошибку — логируем
                    _logger.LogWarning("Не удалось сохранить статус ошибки для {InstanceId}", instanceId);
                }

                _logger.LogError(ex,
                    "Процесс {ProcessId}: ошибка выполнения",
                    request.ProcessPublicId);
            }
            finally
            {
                // Удаляем запрос из очереди
                _bpmState.DequeueRequest(request);
            }
        }
    }
}
