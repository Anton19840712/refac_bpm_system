using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки OpenTelemetry метрик и мониторинга
    /// </summary>
    public sealed class TelemetryOptions
    {
        /// <summary>
        /// Имя сервиса для телеметрии
        /// </summary>
        public string ServiceName { get; set; } = "UnknownService";

        /// <summary>
        /// Версия сервиса
        /// </summary>
        public string ServiceVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Namespace сервиса
        /// </summary>
        public string ServiceNamespace { get; set; } = "PIT";

        /// <summary>
        /// Включить метрики ASP.NET Core
        /// </summary>
        public bool EnableAspNetCoreInstrumentation { get; set; } = true;

        /// <summary>
        /// Включить метрики HTTP клиента
        /// </summary>
        public bool EnableHttpClientInstrumentation { get; set; } = true;

        /// <summary>
        /// Включить метрики .NET Runtime
        /// </summary>
        public bool EnableRuntimeInstrumentation { get; set; } = true;

        /// <summary>
        /// Включить экспорт в консоль (для отладки)
        /// </summary>
        public bool EnableConsoleExporter { get; set; } = false;

        /// <summary>
        /// Включить OTLP экспортер
        /// </summary>
        public bool EnableOtlpExporter { get; set; } = true;

        /// <summary>
        /// OTLP Endpoint
        /// </summary>
        public string OtlpEndpoint { get; set; } = "http://localhost:4317";

        /// <summary>
        /// Протокол OTLP (Grpc, HttpProtobuf)
        /// </summary>
        public string OtlpProtocol { get; set; } = "Grpc";

        /// <summary>
        /// Включить собственные метрики приложения
        /// </summary>
        public bool EnableCustomMetrics { get; set; } = true;
    }
}
