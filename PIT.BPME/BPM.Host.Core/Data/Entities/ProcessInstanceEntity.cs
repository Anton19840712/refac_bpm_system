namespace BPME.BPM.Host.Core.Data.Entities
{
    /// <summary>
    /// Сущность экземпляра процесса для хранения в БД.
    ///
    /// Хранит информацию о каждом запуске процесса:
    /// - Статус выполнения
    /// - Входные/выходные данные
    /// - Время начала/завершения
    /// - Ошибки
    /// </summary>
    public class ProcessInstanceEntity
    {
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
        /// Статус выполнения: Pending, Running, Completed, Failed
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Входные аргументы (JSON)
        /// </summary>
        public string? InputArgumentsJson { get; set; }

        /// <summary>
        /// Результат выполнения (JSON)
        /// </summary>
        public string? OutputResultJson { get; set; }

        /// <summary>
        /// Сообщение об ошибке (если Failed)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Источник запроса
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Время создания запроса
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Время начала выполнения
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Время завершения
        /// </summary>
        public DateTime? CompletedAt { get; set; }
    }
}
