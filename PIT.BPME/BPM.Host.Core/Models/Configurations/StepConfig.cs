namespace BPME.BPM.Host.Core.Models.Configurations
{
    /// <summary>
    /// Конфигурация шага процесса.
    ///
    /// Это DTO с настройками шага — ЧТО делать.
    /// Логика КАК делать находится в StepExecutorService.
    ///
    /// Хранится в БД, загружается при запуске процесса.
    /// </summary>
    public class StepConfig
    {
        /// <summary>
        /// Публичный идентификатор шага (уникален в рамках процесса)
        /// </summary>
        public string PublicId { get; set; } = string.Empty;

        /// <summary>
        /// Название шага (для отображения)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание шага
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Тип шага — определяет, какой исполнитель будет использоваться.
        ///
        /// Примеры:
        /// - "HttpRequest" — отправка HTTP-запроса
        /// - "RabbitMQ" — отправка в RabbitMQ
        /// - "SubProcess" — вызов другого процесса
        /// - "Script" — выполнение скрипта
        /// </summary>
        public string StepType { get; set; } = "Default";

        /// <summary>
        /// Идентификаторы следующих шагов.
        /// Если несколько — шаги выполняются параллельно.
        /// Если пусто — это конечный шаг.
        /// </summary>
        public List<string>? NextStepIds { get; set; }

        /// <summary>
        /// Настройки шага в формате JSON.
        /// Содержимое зависит от StepType.
        ///
        /// Пример для HttpRequest:
        /// {
        ///   "url": "https://api.example.com/data",
        ///   "method": "POST",
        ///   "headers": { "Authorization": "Bearer ..." }
        /// }
        /// </summary>
        public Dictionary<string, object>? Settings { get; set; }

        /// <summary>
        /// Маппинг входных данных.
        /// Определяет, откуда брать InputValue для этого шага.
        ///
        /// Примеры:
        /// - "$.processInput" — из начальных аргументов процесса
        /// - "$.steps.step1.output" — из результата шага step1
        /// </summary>
        public string? InputMapping { get; set; }

        /// <summary>
        /// Таймаут выполнения шага (в секундах)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Количество попыток при ошибке
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Порядок сортировки (для отображения)
        /// </summary>
        public int Order { get; set; }
    }
}
