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
        /// Имя очереди для получения запросов на запуск процессов
        /// </summary>
        public string QueueName { get; set; } = "bpm.process.start";

        /// <summary>
        /// Включён ли listener (можно отключить через конфигурацию)
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
    }
}
