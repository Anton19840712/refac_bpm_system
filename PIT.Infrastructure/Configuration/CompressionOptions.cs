using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки сжатия ответов
    /// </summary>
    public sealed class CompressionOptions
    {
        /// <summary>
        /// Включить Brotli сжатие
        /// </summary>
        public bool EnableBrotli { get; set; } = true;

        /// <summary>
        /// Уровень сжатия Brotli
        /// </summary>
        public CompressionLevel BrotliLevel { get; set; } = CompressionLevel.Fastest;

        /// <summary>
        /// Включить Gzip сжатие
        /// </summary>
        public bool EnableGzip { get; set; } = true;

        /// <summary>
        /// Уровень сжатия Gzip
        /// </summary>
        public CompressionLevel GzipLevel { get; set; } = CompressionLevel.Fastest;

        /// <summary>
        /// Включить сжатие для HTTPS
        /// </summary>
        public bool EnableForHttps { get; set; } = true;

        /// <summary>
        /// MIME типы для сжатия
        /// </summary>
        public string[] MimeTypes { get; set; } =
        [
            "text/plain",
        "text/css",
        "text/html",
        "text/xml",
        "text/json",
        "application/json",
        "application/javascript",
        "application/xml",
        "application/problem+json"
        ];
    }
}
