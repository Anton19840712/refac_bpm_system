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
        public long Id { get; set; }
        public string InstanceId { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }
        public string ProcessPublicId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public object? InputArguments { get; set; }
        public object? OutputResult { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Source { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
