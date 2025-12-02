namespace BPME.BPM.Host.Core.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с экземплярами процессов.
    /// </summary>
    public interface IProcessInstanceRepository
    {
        /// <summary>
        /// Создать новый экземпляр процесса
        /// </summary>
        Task<ProcessInstanceDto> CreateAsync(ProcessInstanceDto instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить экземпляр процесса
        /// </summary>
        Task<ProcessInstanceDto> UpdateAsync(ProcessInstanceDto instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить по InstanceId
        /// </summary>
        Task<ProcessInstanceDto?> GetByInstanceIdAsync(string instanceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить по CorrelationId
        /// </summary>
        Task<ProcessInstanceDto?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить все экземпляры по ProcessPublicId
        /// </summary>
        Task<IEnumerable<ProcessInstanceDto>> GetByProcessPublicIdAsync(string processPublicId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить последние N экземпляров
        /// </summary>
        Task<IEnumerable<ProcessInstanceDto>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO для экземпляра процесса
    /// </summary>
    public class ProcessInstanceDto
    {
        /// <summary>
        /// Внутренний идентификатор записи в БД
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Уникальный идентификатор экземпляра процесса
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор корреляции (для связи с внешними системами)
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// PublicId конфигурации процесса
        /// </summary>
        public string ProcessPublicId { get; set; } = string.Empty;

        /// <summary>
        /// Статус выполнения (Pending, Running, Completed, Failed)
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Входные аргументы процесса
        /// </summary>
        public object? InputArguments { get; set; }

        /// <summary>
        /// Результат выполнения процесса
        /// </summary>
        public object? OutputResult { get; set; }

        /// <summary>
        /// Сообщение об ошибке (если статус Failed)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Источник запуска (API, RabbitMQ, TestRunner)
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Время создания экземпляра
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Время начала выполнения
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Время завершения выполнения
        /// </summary>
        public DateTime? CompletedAt { get; set; }
    }
}
