namespace BPME.BPM.Host.Core.Models.States
{
    /// <summary>
    /// Модель состояния процесса.
    ///
    /// Хранит общее состояние всего процесса:
    /// - Начальные аргументы (с чем запустили процесс)
    /// - Коллекцию состояний всех шагов
    /// - Статус всего процесса
    /// - Общие параметры, доступные всем шагам
    ///
    /// На схеме это ProcessStateService с коллекцией StepState (1, 2, 3... n)
    /// </summary>
    public class ProcessStateModel
    {
        /// <summary>
        /// Уникальный идентификатор запущенного процесса
        /// </summary>
        public Guid ProcessInstanceId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Публичный идентификатор процесса (из конфигурации)
        /// </summary>
        public string PublicProcessId { get; set; } = string.Empty;

        /// <summary>
        /// Входные аргументы процесса.
        /// То, с чем процесс был запущен (например, данные из API или шины данных).
        /// </summary>
        public object? InputArguments { get; set; }

        /// <summary>
        /// Финальный результат процесса.
        /// Заполняется после выполнения последнего шага.
        /// </summary>
        public object? OutputResult { get; set; }

        /// <summary>
        /// Статус выполнения процесса
        /// </summary>
        public ProcessExecutionStatus Status { get; set; } = ProcessExecutionStatus.Pending;

        /// <summary>
        /// Время запуска процесса
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Время завершения процесса
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Состояния всех шагов процесса.
        /// Ключ — PublicStepId, значение — состояние шага.
        ///
        /// Это та самая коллекция StepState (1, 2, 3... n) со схемы.
        /// </summary>
        public Dictionary<string, StepStateModel> StepStates { get; set; } = new();

        /// <summary>
        /// Глобальные параметры процесса.
        /// Доступны всем шагам для чтения и записи.
        /// </summary>
        public Dictionary<string, object?> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Статусы выполнения процесса
    /// </summary>
    public enum ProcessExecutionStatus
    {
        /// <summary>Ожидает выполнения</summary>
        Pending,

        /// <summary>Выполняется</summary>
        Running,

        /// <summary>Успешно завершён</summary>
        Completed,

        /// <summary>Завершён с ошибкой</summary>
        Failed,

        /// <summary>Отменён</summary>
        Cancelled
    }
}
