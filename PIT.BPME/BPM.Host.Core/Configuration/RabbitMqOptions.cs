namespace BPME.BPM.Host.Core.Configuration
{
    /// <summary>
    /// Настройки подключения к RabbitMQ.
    ///
    /// Загружаются из appsettings.json секции "RabbitMQ".
    /// </summary>
    public class RabbitMqOptions
    {
        /// <summary>
        /// Имя секции в конфигурации
        /// </summary>
        public const string SectionName = "RabbitMQ";

        /// <summary>
        /// Хост RabbitMQ (по умолчанию localhost)
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Порт (по умолчанию 5672)
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Username { get; set; } = "guest";

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Virtual host
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// Включены ли слушатели RabbitMQ
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Количество повторных попыток подключения
        /// </summary>
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// Задержка между попытками подключения (секунды)
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 5;

        /// <summary>
        /// Список очередей для прослушивания.
        /// Каждая очередь = отдельная интеграция.
        /// </summary>
        public List<QueueListenerConfig> Queues { get; set; } = new();
    }

    /// <summary>
    /// Конфигурация отдельной очереди для прослушивания
    /// </summary>
    public class QueueListenerConfig
    {
        /// <summary>
        /// Имя очереди в RabbitMQ
        /// </summary>
        public string QueueName { get; set; } = string.Empty;

        /// <summary>
        /// PublicId процесса, который будет запускаться для сообщений из этой очереди.
        /// Если null - используется processPublicId из самого сообщения.
        /// </summary>
        public string? ProcessPublicId { get; set; }

        /// <summary>
        /// Включён ли этот слушатель
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Описание интеграции (для логов и документации)
        /// </summary>
        public string? Description { get; set; }
    }
}
