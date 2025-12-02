using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки CORS политик
    /// </summary>
    public sealed class CorsOptions
    {
        /// <summary>
        /// Имя политики по умолчанию
        /// </summary>
        public string DefaultPolicyName { get; set; } = "DefaultCorsPolicy";

        /// <summary>
        /// Разрешённые источники
        /// </summary>
        public string[] AllowedOrigins { get; set; } = ["*"];

        /// <summary>
        /// Разрешённые методы
        /// </summary>
        public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"];

        /// <summary>
        /// Разрешённые заголовки
        /// </summary>
        public string[] AllowedHeaders { get; set; } = ["*"];

        /// <summary>
        /// Разрешить учётные данные
        /// </summary>
        public bool AllowCredentials { get; set; } = false;

        /// <summary>
        /// Заголовки для экспонирования
        /// </summary>
        public string[] ExposedHeaders { get; set; } = ["X-Correlation-ID", "X-Request-ID"];

        /// <summary>
        /// Время кэширования preflight запросов (в секундах)
        /// </summary>
        public int PreflightMaxAge { get; set; } = 3600;
    }
}
