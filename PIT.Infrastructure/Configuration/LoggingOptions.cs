using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки структурированного логирования Serilog
    /// </summary>
    public sealed class LoggingOptions
    {
        /// <summary>
        /// Имя сервиса (для обогащения логов)
        /// </summary>
        public string ServiceName { get; set; } = "UnknownService";

        /// <summary>
        /// Окружение (Development, Staging, Production)
        /// </summary>
        public string Environment { get; set; } = "Development";

        /// <summary>
        /// Минимальный уровень логирования
        /// </summary>
        public string MinimumLevel { get; set; } = "Information";

        /// <summary>
        /// Включить консольный вывод
        /// </summary>
        public bool EnableConsole { get; set; } = true;

        /// <summary>
        /// Включить запись в файл
        /// </summary>
        public bool EnableFile { get; set; } = false;

        /// <summary>
        /// Путь к файлу логов
        /// </summary>
        public string? FilePath { get; set; } = "logs/log-.txt";

        /// <summary>
        /// Период ротации файлов логов
        /// </summary>
        public string RollingInterval { get; set; } = "Day";

        /// <summary>
        /// Максимальное количество файлов логов
        /// </summary>
        public int? RetainedFileCountLimit { get; set; } = 31;

        /// <summary>
        /// Формат вывода логов (Compact, Json, Text)
        /// </summary>
        public string OutputFormat { get; set; } = "Json";

        /// <summary>
        /// Включить обогащение логов информацией об окружении
        /// </summary>
        public bool EnrichWithEnvironment { get; set; } = true;

        /// <summary>
        /// Включить обогащение логов информацией о потоке
        /// </summary>
        public bool EnrichWithThread { get; set; } = true;

        /// <summary>
        /// Включить обогащение логов информацией о процессе
        /// </summary>
        public bool EnrichWithProcess { get; set; } = true;

        /// <summary>
        /// Включить обогащение логов Correlation ID
        /// </summary>
        public bool EnrichWithCorrelationId { get; set; } = true;
    }
}
