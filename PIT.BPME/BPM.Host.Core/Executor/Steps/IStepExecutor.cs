namespace BPME.BPM.Host.Core.Executor.Steps
{
    /// <summary>
    /// Интерфейс исполнителя шага.
    /// Каждый тип шага имеет свой Executor, который реализует этот интерфейс.
    /// </summary>
    public interface IStepExecutor
    {
        /// <summary>
        /// Тип шага, который обрабатывает этот Executor.
        /// Используется для маппинга StepType → Executor.
        /// </summary>
        string StepType { get; }

        /// <summary>
        /// Выполнить шаг
        /// </summary>
        /// <param name="settingsJson">Настройки шага в формате JSON</param>
        /// <param name="inputData">Входные данные процесса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат выполнения шага</returns>
        Task<StepExecutionResult> ExecuteAsync(
            string? settingsJson,
            Dictionary<string, object>? inputData,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Результат выполнения шага
    /// </summary>
    public class StepExecutionResult
    {
        /// <summary>
        /// Успешно ли выполнен шаг
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Выходные данные шага
        /// </summary>
        public object? Output { get; set; }

        /// <summary>
        /// Сообщение об ошибке (если не успешно)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Время выполнения
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Создать успешный результат
        /// </summary>
        /// <param name="output">Выходные данные</param>
        /// <param name="duration">Время выполнения</param>
        public static StepExecutionResult Success(object? output = null, TimeSpan? duration = null)
            => new() { IsSuccess = true, Output = output, Duration = duration ?? TimeSpan.Zero };

        /// <summary>
        /// Создать результат с ошибкой
        /// </summary>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        /// <param name="duration">Время выполнения</param>
        public static StepExecutionResult Failure(string errorMessage, TimeSpan? duration = null)
            => new() { IsSuccess = false, ErrorMessage = errorMessage, Duration = duration ?? TimeSpan.Zero };
    }
}
