using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PIT.Infrastructure.Configuration;

namespace PIT.Infrastructure.Compression
{
    /// <summary>
    /// Расширения для настройки сжатия ответов
    /// </summary>
    public static class CompressionExtensions
    {
        /// <summary>
        /// Добавляет сжатие ответов (Gzip и Brotli)
        /// </summary>
        public static IServiceCollection AddPitCompression(
            this IServiceCollection services,
            IOptions<InfrastructureOptions> options)
        {
            var compressionOptions = options.Value.Compression;

            services.AddResponseCompression(responseCompressionOptions =>
            {
                responseCompressionOptions.EnableForHttps = compressionOptions.EnableForHttps;

                var providers = new List<string>();

                if (compressionOptions.EnableBrotli)
                {
                    providers.Add("br");
                }

                if (compressionOptions.EnableGzip)
                {
                    providers.Add("gzip");
                }

                responseCompressionOptions.Providers.Clear();
                foreach (var provider in providers)
                {
                    if (provider == "br")
                    {
                        responseCompressionOptions.Providers.Add<BrotliCompressionProvider>();
                    }
                    else if (provider == "gzip")
                    {
                        responseCompressionOptions.Providers.Add<GzipCompressionProvider>();
                    }
                }

                responseCompressionOptions.MimeTypes = compressionOptions.MimeTypes;
            });

            if (compressionOptions.EnableBrotli)
            {
                services.Configure<BrotliCompressionProviderOptions>(brotliOptions =>
                {
                    brotliOptions.Level = compressionOptions.BrotliLevel;
                });
            }

            if (compressionOptions.EnableGzip)
            {
                services.Configure<GzipCompressionProviderOptions>(gzipOptions =>
                {
                    gzipOptions.Level = compressionOptions.GzipLevel;
                });
            }

            return services;
        }

        /// <summary>
        /// Использует сжатие ответов
        /// </summary>
        public static IApplicationBuilder UsePitCompression(this IApplicationBuilder app)
        {
            app.UseResponseCompression();
            return app;
        }
    }
}
