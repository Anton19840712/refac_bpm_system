using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки Health Checks
    /// </summary>
    public sealed class HealthCheckOptions
    {
        /// <summary>
        /// Endpoint для health check
        /// </summary>
        public string Endpoint { get; set; } = "/health";

        /// <summary>
        /// Endpoint для readiness check
        /// </summary>
        public string ReadinessEndpoint { get; set; } = "/health/ready";

        /// <summary>
        /// Endpoint для liveness check
        /// </summary>
        public string LivenessEndpoint { get; set; } = "/health/live";

        /// <summary>
        /// Включить проверку базы данных
        /// </summary>
        public bool EnableDatabaseCheck { get; set; } = true;

        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        public string? DatabaseConnectionString { get; set; }

        /// <summary>
        /// Тип базы данных (PostgreSql, SqlServer, MySql)
        /// </summary>
        public string DatabaseType { get; set; } = "PostgreSql";

        /// <summary>
        /// Таймаут проверки (в секундах)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Включить детальный вывод
        /// </summary>
        public bool EnableDetailedOutput { get; set; } = true;
    }
}
