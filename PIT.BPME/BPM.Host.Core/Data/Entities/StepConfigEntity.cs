namespace BPME.BPM.Host.Core.Data.Entities
{
    /// <summary>
    /// Entity конфигурации шага для EF Core.
    ///
    /// Таблица: step_configs
    /// </summary>
    public class StepConfigEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Внешний ключ на процесс
        /// </summary>
        public Guid ProcessConfigId { get; set; }

        /// <summary>
        /// Публичный идентификатор шага (уникален в рамках процесса)
        /// </summary>
        public string PublicId { get; set; } = string.Empty;

        /// <summary>
        /// Название шага
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Тип шага (HttpRequest, RabbitMQ, SubProcess, etc.)
        /// </summary>
        public string StepType { get; set; } = "Default";

        /// <summary>
        /// Идентификаторы следующих шагов (JSON массив)
        /// </summary>
        public string? NextStepIdsJson { get; set; }

        /// <summary>
        /// Настройки шага (JSON)
        /// </summary>
        public string? SettingsJson { get; set; }

        /// <summary>
        /// Маппинг входных данных
        /// </summary>
        public string? InputMapping { get; set; }

        /// <summary>
        /// Таймаут шага (секунды)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Количество повторных попыток
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Порядок сортировки
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Навигационное свойство — родительский процесс
        /// </summary>
        public ProcessConfigEntity? ProcessConfig { get; set; }
    }
}
