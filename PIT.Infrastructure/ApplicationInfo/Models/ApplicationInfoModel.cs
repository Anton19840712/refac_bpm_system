using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ApplicationInfo.Models
{
    /// <summary>
    /// Модель с информацией о приложении
    /// </summary>
    public sealed class ApplicationInfoModel
    {
        /// <summary>
        /// Имя приложения
        /// </summary>
        public string ApplicationName { get; set; } = null!;

        /// <summary>
        /// Версия приложения
        /// </summary>
        public string Version { get; set; } = null!;

        /// <summary>
        /// Окружение
        /// </summary>
        public string Environment { get; set; } = null!;

        /// <summary>
        /// Имя хоста
        /// </summary>
        public string HostName { get; set; } = null!;

        /// <summary>
        /// Версия .NET
        /// </summary>
        public string DotNetVersion { get; set; } = null!;

        /// <summary>
        /// Операционная система
        /// </summary>
        public string OperatingSystem { get; set; } = null!;

        /// <summary>
        /// Архитектура процессора
        /// </summary>
        public string ProcessorArchitecture { get; set; } = null!;

        /// <summary>
        /// Время запуска приложения
        /// </summary>
        public DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Время работы приложения
        /// </summary>
        public TimeSpan Uptime { get; set; }

        /// <summary>
        /// Информация о базе данных
        /// </summary>
        public DatabaseInfo? Database { get; set; }

        /// <summary>
        /// Информация о компании производители 
        /// </summary>
        public string Company { get; private set; } = $"© {DateTime.Now.Year} ПРОТЕЙ Ай-Ти-Инжиниринг";
    }
}
