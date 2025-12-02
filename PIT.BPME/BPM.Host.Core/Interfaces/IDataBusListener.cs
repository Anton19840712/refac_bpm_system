namespace BPME.BPM.Host.Core.Interfaces
{
    /// <summary>
    /// Слушатель шины данных.
    ///
    /// Получает сообщения из внешней системы (RabbitMQ, Kafka, etc.)
    /// и преобразует их в StartRequest для запуска процессов.
    ///
    /// На схеме это IDataBusListener.
    ///
    /// Реализации:
    /// - RabbitMqListener — для RabbitMQ
    /// - KafkaListener — для Kafka (в будущем)
    ///
    /// Lifetime: HostedService (Singleton)
    /// </summary>
    public interface IDataBusListener
    {
        /// <summary>
        /// Запустить прослушивание шины данных.
        /// Вызывается при старте приложения.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        Task StartListeningAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Остановить прослушивание.
        /// Вызывается при остановке приложения.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        Task StopListeningAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Статус подключения к шине данных.
        /// </summary>
        bool IsConnected { get; }
    }
}
