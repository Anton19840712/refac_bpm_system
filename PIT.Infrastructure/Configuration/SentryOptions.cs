using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки Sentry мониторинга
    /// </summary>
    public sealed class SentryOptions
    {
        /// <summary>
        /// DSN для Sentry
        /// </summary>
        public string? Dsn { get; set; }

        /// <summary>
        /// Окружение (development, staging, production)
        /// </summary>
        public string Environment { get; set; } = "development";

        /// <summary>
        /// Частота отправки трассировок (0.0 - 1.0)
        /// </summary>
        public double TracesSampleRate { get; set; } = 1.0;

        /// <summary>
        /// Включить профилирование
        /// </summary>
        public bool EnableProfiling { get; set; } = true;

        /// <summary>
        /// Частота профилирования (0.0 - 1.0)
        /// </summary>
        public double ProfilesSampleRate { get; set; } = 1.0;

        /// <summary>
        /// Минимальный уровень событий для отправки
        /// </summary>
        public string MinimumEventLevel { get; set; } = "Warning";

        /// <summary>
        /// Минимальный уровень breadcrumbs
        /// </summary>
        public string MinimumBreadcrumbLevel { get; set; } = "Information";

        /// <summary>
        /// Включить интеграцию с Serilog
        /// </summary>
        public bool EnableSerilogIntegration { get; set; } = true;

        /// <summary>
        /// Захватывать необработанные исключения
        /// </summary>
        public bool CaptureUnhandledExceptions { get; set; } = true;

        /// <summary>
        /// Отправлять PII данные
        /// </summary>
        public bool SendDefaultPii { get; set; } = false;

        /// <summary>
        /// Максимальное количество breadcrumbs
        /// </summary>
        public int MaxBreadcrumbs { get; set; } = 100;
    }
}
