namespace BPME.BPM.Host.Core.Models.States
{
    /// <summary>
    /// Модель состояния шага процесса.
    ///
    /// Хранит всё, что связано с выполнением конкретного шага:
    /// - Входные данные (что получили от предыдущего шага)
    /// - Выходные данные (что передадим следующему шагу)
    /// - Статус выполнения
    /// - Параметры, которые шаг может использовать
    /// </summary>
    public class StepStateModel
    {
        /// <summary>
        /// Идентификатор шага
        /// </summary>
        public string PublicStepId { get; set; } = string.Empty;

        /// <summary>
        /// Входные данные для шага.
        /// Это то, что передал предыдущий шаг (или начальные аргументы процесса).
        /// </summary>
        public object? InputValue { get; set; }

        /// <summary>
        /// Выходные данные шага.
        /// Результат выполнения, который будет передан следующим шагам.
        /// </summary>
        public object? OutputValue { get; set; }

        /// <summary>
        /// Статус выполнения шага
        /// </summary>
        public StepExecutionStatus Status { get; set; } = StepExecutionStatus.Pending;

        /// <summary>
        /// Время начала выполнения
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Время завершения выполнения
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Сообщение об ошибке (если Status = Failed)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Дополнительные параметры шага (ключ-значение).
        /// Используется для передачи произвольных данных.
        /// </summary>
        public Dictionary<string, object?> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Статусы выполнения шага
    /// </summary>
    public enum StepExecutionStatus
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
