using Microsoft.Extensions.Logging;

namespace BPME.BPM.Host.Core.Executor.Steps
{
    /// <summary>
    /// Фабрика для получения executor'а по типу шага.
    ///
    /// Регистрация в DI:
    /// builder.Services.AddScoped&lt;IStepExecutor, SampleStepExecutor&gt;();
    /// builder.Services.AddScoped&lt;IStepExecutor, HttpRequestStepExecutor&gt;();
    /// builder.Services.AddScoped&lt;IStepExecutor, ScriptStepExecutor&gt;();
    /// builder.Services.AddScoped&lt;StepExecutorFactory&gt;();
    /// </summary>
    public class StepExecutorFactory
    {
        private readonly Dictionary<string, IStepExecutor> _executors;
        private readonly ILogger<StepExecutorFactory> _logger;

        public StepExecutorFactory(
            IEnumerable<IStepExecutor> executors,
            ILogger<StepExecutorFactory> logger)
        {
            _logger = logger;

            // Строим словарь: StepType → Executor
            _executors = executors.ToDictionary(
                e => e.StepType,
                e => e,
                StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug(
                "StepExecutorFactory: зарегистрировано {Count} executor'ов: {Types}",
                _executors.Count,
                string.Join(", ", _executors.Keys));
        }

        /// <summary>
        /// Список поддерживаемых типов шагов
        /// </summary>
        public IReadOnlyCollection<string> SupportedStepTypes => _executors.Keys;

        /// <summary>
        /// Получить executor по типу шага
        /// </summary>
        /// <param name="stepType">Тип шага (например: "http-request", "script")</param>
        /// <returns>Executor или null если не найден</returns>
        public IStepExecutor? GetExecutor(string stepType)
        {
            if (string.IsNullOrEmpty(stepType))
            {
                _logger.LogWarning("Запрошен executor с пустым stepType");
                return null;
            }

            if (_executors.TryGetValue(stepType, out var executor))
            {
                return executor;
            }

            _logger.LogWarning(
                "Executor для типа '{StepType}' не найден. Доступные: {Available}",
                stepType,
                string.Join(", ", _executors.Keys));

            return null;
        }

        /// <summary>
        /// Получить executor или выбросить исключение
        /// </summary>
        public IStepExecutor GetExecutorOrThrow(string stepType)
        {
            return GetExecutor(stepType)
                   ?? throw new InvalidOperationException(
                       $"Executor для типа '{stepType}' не зарегистрирован. " +
                       $"Доступные типы: {string.Join(", ", _executors.Keys)}");
        }

        /// <summary>
        /// Проверить, поддерживается ли тип шага
        /// </summary>
        public bool IsSupported(string stepType)
        {
            return !string.IsNullOrEmpty(stepType)
                   && _executors.ContainsKey(stepType);
        }
    }
}
