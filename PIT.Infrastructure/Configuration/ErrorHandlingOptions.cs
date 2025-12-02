using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки обработки ошибок по RFC 9457
    /// </summary>
    public sealed class ErrorHandlingOptions
    {
        /// <summary>
        /// Включить детальные сообщения об ошибках (только для Development)
        /// </summary>
        public bool IncludeExceptionDetails { get; set; } = false;

        /// <summary>
        /// Базовый URL для документации типов проблем
        /// </summary>
        public string ProblemDetailsBaseUrl { get; set; } = "https://api.pit.local/problems";

        /// <summary>
        /// Включить трассировку запросов (TraceId в ответах)
        /// </summary>
        public bool IncludeTraceId { get; set; } = true;

        /// <summary>
        /// Включить интеграцию с FluentValidation
        /// </summary>
        public bool EnableFluentValidation { get; set; } = true;

        /// <summary>
        /// Имя заголовка для Correlation ID
        /// </summary>
        public string CorrelationIdHeader { get; set; } = "X-Correlation-ID";
    }
}
