namespace BPME.BPM.Host.Core.Data.Entities
{
    /// <summary>
    /// Entity конфигурации процесса для EF Core.
    ///
    /// Таблица: process_configs
    /// </summary>
    public class ProcessConfigEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Публичный идентификатор (уникальный)
        /// </summary>
        public string PublicId { get; set; } = string.Empty;

        /// <summary>
        /// Название процесса
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Версия конфигурации
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Активна ли конфигурация
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Идентификатор стартового шага
        /// </summary>
        public string? StartStepId { get; set; }

        /// <summary>
        /// Настройки в формате JSON
        /// </summary>
        public string? SettingsJson { get; set; }

        /// <summary>
        /// Таймаут процесса (секунды)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Навигационное свойство — шаги процесса
        /// </summary>
        public List<StepConfigEntity> Steps { get; set; } = new();
    }
}
