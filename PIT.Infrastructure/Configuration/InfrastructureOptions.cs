using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Главный класс конфигурации инфраструктурного пакета.
    /// Все компоненты включаются через feature flags.
    /// </summary>
    public sealed class InfrastructureOptions
    {
        public const string SectionName = "Infrastructure";

        /// <summary>
        /// Включить обработку ошибок по RFC 9457
        /// </summary>
        public bool EnableErrorHandling { get; set; } = true;

        /// <summary>
        /// Настройки обработки ошибок
        /// </summary>
        public ErrorHandlingOptions ErrorHandling { get; set; } = new();

        /// <summary>
        /// Включить сжатие ответов (gzip, brotli)
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Настройки сжатия
        /// </summary>
        public CompressionOptions Compression { get; set; } = new();

        /// <summary>
        /// Включить структурированное логирование Serilog
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Настройки логирования
        /// </summary>
        public LoggingOptions Logging { get; set; } = new();

        /// <summary>
        /// Включить OpenTelemetry метрики и мониторинг
        /// </summary>
        public bool EnableTelemetry { get; set; } = true;

        /// <summary>
        /// Настройки телеметрии
        /// </summary>
        public TelemetryOptions Telemetry { get; set; } = new();

        /// <summary>
        /// Включить CORS политики
        /// </summary>
        public bool EnableCors { get; set; } = false;

        /// <summary>
        /// Настройки CORS
        /// </summary>
        public CorsOptions Cors { get; set; } = new();

        /// <summary>
        /// Включить Health Checks
        /// </summary>
        public bool EnableHealthChecks { get; set; } = true;

        /// <summary>
        /// Настройки Health Checks
        /// </summary>
        public HealthCheckOptions HealthCheck { get; set; } = new();

        /// <summary>
        /// Включить Sentry мониторинг и профилирование
        /// </summary>
        public bool EnableSentry { get; set; } = false;

        /// <summary>
        /// Настройки Sentry
        /// </summary>
        public SentryOptions Sentry { get; set; } = new();

        /// <summary>
        /// Включить информацию о приложении
        /// </summary>
        public bool EnableApplicationInfo { get; set; } = true;

        /// <summary>
        /// Включить Swagger/OpenAPI документацию
        /// </summary>
        public bool EnableSwagger { get; set; } = true;

        /// <summary>
        /// Настройки Swagger
        /// </summary>
        public SwaggerOptions Swagger { get; set; } = new();

        /// <summary>
        /// Включить Security Headers
        /// </summary>
        public bool EnableSecurityHeaders { get; set; } = true;

        /// <summary>
        /// Настройки Security Headers
        /// </summary>
        public SecurityHeadersOptions SecurityHeaders { get; set; } = new();
    }
}
