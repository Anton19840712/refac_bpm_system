using BPME.BPM.Host.Core.Executor.Steps;
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
        private readonly StepExecutorFactory _stepExecutorFactory;
        private object? _outputValue;

        /// <summary>
        /// Создаёт исполнитель для конкретного шага
        /// </summary>
        /// <param name="stepState">Состояние шага (создаётся ProcessExecutorService)</param>
        /// <param name="stepExecutorFactory">Фабрика executor'ов по типу шага</param>
        /// <param name="logger">Логгер</param>
        public StepExecutorService(
            StepState stepState,
            StepExecutorFactory stepExecutorFactory,
            ILogger<StepExecutorService> logger)
        {
            _stepState = stepState;
            _stepExecutorFactory = stepExecutorFactory;
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
        /// Выполняет действие шага через StepExecutorFactory.
        /// Фабрика резолвит нужный IStepExecutor по StepType.
        /// </summary>
        private async Task<object?> ExecuteStepByTypeAsync(StepConfig config, CancellationToken cancellationToken)
        {
            var stepType = config.StepType;

            // Пытаемся найти executor через фабрику
            var executor = _stepExecutorFactory.GetExecutor(stepType);

            if (executor != null)
            {
                // Подготавливаем входные данные как Dictionary
                var inputData = InputValue as Dictionary<string, object>
                    ?? ConvertToInputDictionary(InputValue);

                // Сериализуем Settings в JSON для executor'а
                var settingsJson = config.Settings != null
                    ? System.Text.Json.JsonSerializer.Serialize(config.Settings)
                    : null;

                // Выполняем через IStepExecutor
                var result = await executor.ExecuteAsync(
                    settingsJson,
                    inputData,
                    cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(
                        $"Шаг {config.PublicId} ({stepType}) завершился с ошибкой: {result.ErrorMessage}");
                }

                return result.Output;
            }

            // Fallback: если executor не найден — используем заглушку
            _logger.LogWarning(
                "Шаг {StepId}: executor для типа '{StepType}' не найден, используется заглушка",
                config.PublicId,
                stepType);

            return await ExecuteFallbackAsync(config, cancellationToken);
        }

        /// <summary>
        /// Конвертирует входные данные в Dictionary для IStepExecutor
        /// </summary>
        private Dictionary<string, object>? ConvertToInputDictionary(object? input)
        {
            if (input == null) return null;

            if (input is Dictionary<string, object> dict)
                return dict;

            // Пробуем сконвертировать из JSON
            if (input is System.Text.Json.JsonElement jsonElement)
            {
                var result = new Dictionary<string, object>();
                if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    foreach (var prop in jsonElement.EnumerateObject())
                    {
                        result[prop.Name] = prop.Value;
                    }
                }
                return result;
            }

            // Оборачиваем как единственное значение
            return new Dictionary<string, object> { ["value"] = input };
        }

        /// <summary>
        /// Fallback для неизвестных типов шагов (когда нет executor'а)
        /// </summary>
        private async Task<object?> ExecuteFallbackAsync(StepConfig config, CancellationToken cancellationToken)
        {
            _logger.LogDebug(
                "Шаг {StepId}: fallback выполнение (тип: {StepType})",
                config.PublicId,
                config.StepType);

            await Task.Delay(50, cancellationToken);

            return new
            {
                Input = InputValue,
                StepId = config.PublicId,
                StepType = config.StepType,
                ExecutedAt = DateTime.UtcNow,
                Warning = "Executor not found, fallback used"
            };
        }
    }
}
