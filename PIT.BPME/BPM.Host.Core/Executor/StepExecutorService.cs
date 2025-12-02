using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using BPME.BPM.Host.Core.State;
using Microsoft.Extensions.Logging;

namespace BPME.BPM.Host.Core.Executor
{
    /// <summary>
    /// Сервис выполнения шага процесса.
    ///
    /// Отвечает за:
    /// - Подготовку входных данных для шага
    /// - Выполнение действия шага (в зависимости от типа)
    /// - Сохранение результата в состояние
    /// - Обработку ошибок и повторные попытки
    ///
    /// Паттерн: Strategy — разные типы шагов выполняются по-разному.
    ///
    /// Lifetime: Scoped (создаётся для каждого выполняемого шага)
    /// </summary>
    public class StepExecutorService : IExecutor<StepConfig>
    {
        private readonly ILogger<StepExecutorService> _logger;
        private readonly StepState _stepState;
        private object? _outputValue;

        /// <summary>
        /// Создаёт исполнитель для конкретного шага
        /// </summary>
        /// <param name="stepState">Состояние шага (создаётся ProcessExecutorService)</param>
        /// <param name="logger">Логгер</param>
        public StepExecutorService(StepState stepState, ILogger<StepExecutorService> logger)
        {
            _stepState = stepState;
            _logger = logger;
        }

        /// <inheritdoc />
        public object? InputValue { get; set; }

        /// <inheritdoc />
        public object? OutputValue => _outputValue;

        /// <inheritdoc />
        public async Task<StepConfig> ExecuteAsync(StepConfig config, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Шаг {StepId} ({StepType}): начало выполнения",
                config.PublicId,
                config.StepType);

            // 1. Устанавливаем входные данные в состояние
            _stepState.SetInput(InputValue);
            _stepState.MarkAsRunning();

            try
            {
                // 2. Выполняем действие в зависимости от типа шага
                _outputValue = await ExecuteStepByTypeAsync(config, cancellationToken);

                // 3. Сохраняем результат в состояние
                _stepState.SetOutput(_outputValue);
                _stepState.MarkAsCompleted();

                _logger.LogInformation(
                    "Шаг {StepId}: успешно завершён",
                    config.PublicId);
            }
            catch (OperationCanceledException)
            {
                _stepState.MarkAsFailed("Выполнение отменено");
                _logger.LogWarning("Шаг {StepId}: выполнение отменено", config.PublicId);
                throw;
            }
            catch (Exception ex)
            {
                _stepState.MarkAsFailed(ex.Message);
                _logger.LogError(ex, "Шаг {StepId}: ошибка выполнения", config.PublicId);
                throw;
            }

            return config;
        }

        /// <summary>
        /// Выполняет действие шага в зависимости от его типа.
        ///
        /// Это точка расширения — здесь добавляются новые типы шагов.
        /// В будущем можно вынести в отдельные классы через DI.
        /// </summary>
        private async Task<object?> ExecuteStepByTypeAsync(StepConfig config, CancellationToken cancellationToken)
        {
            return config.StepType.ToLowerInvariant() switch
            {
                "httprequest" => await ExecuteHttpRequestAsync(config, cancellationToken),
                "rabbitmq" => await ExecuteRabbitMQAsync(config, cancellationToken),
                "subprocess" => await ExecuteSubProcessAsync(config, cancellationToken),
                "script" => await ExecuteScriptAsync(config, cancellationToken),
                "delay" => await ExecuteDelayAsync(config, cancellationToken),
                _ => await ExecuteDefaultAsync(config, cancellationToken)
            };
        }

        #region Реализации типов шагов (заглушки для примера)

        /// <summary>
        /// Выполнение HTTP-запроса.
        /// TODO: Реализовать с использованием HttpClient
        /// </summary>
        private async Task<object?> ExecuteHttpRequestAsync(StepConfig config, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Шаг {StepId}: выполнение HTTP-запроса", config.PublicId);

            // Заглушка — имитация HTTP-запроса
            await Task.Delay(100, cancellationToken);

            return new
            {
                StatusCode = 200,
                Body = $"Response from HTTP step {config.PublicId}",
                ExecutedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Отправка сообщения в RabbitMQ.
        /// TODO: Реализовать с использованием RabbitMQ.Client
        /// </summary>
        private async Task<object?> ExecuteRabbitMQAsync(StepConfig config, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Шаг {StepId}: отправка в RabbitMQ", config.PublicId);

            await Task.Delay(50, cancellationToken);

            return new
            {
                MessageId = Guid.NewGuid(),
                Published = true,
                ExecutedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Вызов подпроцесса.
        /// TODO: Реализовать через рекурсивный вызов ProcessExecutorService
        /// </summary>
        private async Task<object?> ExecuteSubProcessAsync(StepConfig config, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Шаг {StepId}: запуск подпроцесса", config.PublicId);

            await Task.Delay(100, cancellationToken);

            return new
            {
                SubProcessId = Guid.NewGuid(),
                Status = "Completed",
                ExecutedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Выполнение скрипта.
        /// TODO: Реализовать интерпретатор скриптов
        /// </summary>
        private async Task<object?> ExecuteScriptAsync(StepConfig config, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Шаг {StepId}: выполнение скрипта", config.PublicId);

            await Task.Delay(50, cancellationToken);

            return new
            {
                ScriptResult = "OK",
                ExecutedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Задержка (для тестирования).
        /// </summary>
        private async Task<object?> ExecuteDelayAsync(StepConfig config, CancellationToken cancellationToken)
        {
            var delayMs = 1000;
            if (config.Settings?.TryGetValue("delayMs", out var value) == true && value is int ms)
            {
                delayMs = ms;
            }

            _logger.LogDebug("Шаг {StepId}: задержка {DelayMs}ms", config.PublicId, delayMs);

            await Task.Delay(delayMs, cancellationToken);

            return new
            {
                DelayMs = delayMs,
                ExecutedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Действие по умолчанию (для неизвестных типов).
        /// </summary>
        private async Task<object?> ExecuteDefaultAsync(StepConfig config, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Шаг {StepId}: выполнение (тип: {StepType})", config.PublicId, config.StepType);

            await Task.Delay(50, cancellationToken);

            return new
            {
                Input = InputValue,
                StepId = config.PublicId,
                ExecutedAt = DateTime.UtcNow
            };
        }

        #endregion
    }
}
