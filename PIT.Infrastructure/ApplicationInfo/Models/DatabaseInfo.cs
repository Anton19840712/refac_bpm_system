using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ApplicationInfo.Models
{
    /// <summary>
    /// Информация о базе данных
    /// </summary>
    public sealed class DatabaseInfo
    {
        /// <summary>
        /// Тип базы данных
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Версия базы данных
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Хост базы данных
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Порт базы данных
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Имя базы данных
        /// </summary>
        public string? DatabaseName { get; set; }
    }
}
