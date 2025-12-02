namespace BPME.BPM.Host.Core.Models.Configurations
{
    /// <summary>
    /// Запрос на запуск процесса.
    ///
    /// Это DTO, который приходит от клиента (API) или от шины данных.
    /// Содержит информацию о том, какой процесс запустить и с какими данными.
    ///
    /// На схеме это StartRequest, который поступает в IBPMState.
    /// </summary>
    public class StartRequest
    {
        /// <summary>
        /// Публичный идентификатор процесса для запуска.
        /// По этому ID будет найдена конфигурация процесса.
        /// </summary>
        public string ProcessPublicId { get; set; } = string.Empty;

        /// <summary>
        /// Входные аргументы процесса.
        /// Эти данные будут доступны шагам через ProcessState.InputArguments.
        ///
        /// Пример:
        /// {
        ///   "orderId": 12345,
        ///   "customerId": "CUST-001",
        ///   "items": [...]
        /// }
        /// </summary>
        public object? InputArguments { get; set; }

        /// <summary>
        /// Идентификатор корреляции.
        /// Позволяет связать запрос с внешней системой.
        /// Например, ID заказа из внешней системы.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Приоритет выполнения (для очереди)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Источник запроса (для логирования)
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Время создания запроса
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
