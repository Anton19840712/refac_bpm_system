namespace BPME.BPM.Host.Core.Models.Configurations
{
    /// <summary>
    /// Конфигурация бизнес-процесса.
    ///
    /// Это шаблон процесса, который хранится в БД.
    /// При запуске создаётся экземпляр процесса на основе этой конфигурации.
    ///
    /// Аналогия:
    /// - ProcessConfig — это КЛАСС (шаблон)
    /// - Запущенный процесс — это ЭКЗЕМПЛЯР класса
    /// </summary>
    public class ProcessConfig
    {
        /// <summary>
        /// Уникальный идентификатор конфигурации (первичный ключ в БД)
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Публичный идентификатор процесса.
        /// Используется для запуска: "Запустить процесс ORDER_PROCESSING"
        /// </summary>
        public string PublicId { get; set; } = string.Empty;

        /// <summary>
        /// Название процесса (для отображения)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание процесса
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Версия конфигурации.
        /// Позволяет иметь несколько версий одного процесса.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Активна ли эта конфигурация.
        /// Неактивные конфигурации нельзя запускать.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Конфигурации всех шагов процесса
        /// </summary>
        public List<StepConfig> Steps { get; set; } = new();

        /// <summary>
        /// Идентификатор стартового шага.
        /// Если null — определяется автоматически (шаг, на который никто не ссылается).
        /// </summary>
        public string? StartStepId { get; set; }

        /// <summary>
        /// Глобальные настройки процесса
        /// </summary>
        public Dictionary<string, object>? Settings { get; set; }

        /// <summary>
        /// Таймаут всего процесса (в секундах)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Дата создания конфигурации
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего изменения
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
