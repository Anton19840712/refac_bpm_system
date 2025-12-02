using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PIT.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Cors.Extensions
{
    /// <summary>
    /// Расширения для настройки CORS
    /// </summary>
    public static class CorsExtensions
    {
        /// <summary>
        /// Добавляет CORS политики
        /// </summary>
        public static IServiceCollection AddPitCors(
            this IServiceCollection services,
            IOptions<InfrastructureOptions> options)
        {
            var corsOptions = options.Value.Cors;

            services.AddCors(corsBuilder =>
            {
                corsBuilder.AddPolicy(corsOptions.DefaultPolicyName, policy =>
                {
                    // Настройка Origins
                    if (corsOptions.AllowedOrigins.Length == 1 && corsOptions.AllowedOrigins[0] == "*")
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        policy.WithOrigins(corsOptions.AllowedOrigins);
                    }

                    // Настройка Methods
                    if (corsOptions.AllowedMethods.Length == 1 && corsOptions.AllowedMethods[0] == "*")
                    {
                        policy.AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithMethods(corsOptions.AllowedMethods);
                    }

                    // Настройка Headers
                    if (corsOptions.AllowedHeaders.Length == 1 && corsOptions.AllowedHeaders[0] == "*")
                    {
                        policy.AllowAnyHeader();
                    }
                    else
                    {
                        policy.WithHeaders(corsOptions.AllowedHeaders);
                    }

                    // Учётные данные
                    if (corsOptions.AllowCredentials)
                    {
                        policy.AllowCredentials();
                    }

                    // Экспонируемые заголовки
                    if (corsOptions.ExposedHeaders.Length > 0)
                    {
                        policy.WithExposedHeaders(corsOptions.ExposedHeaders);
                    }

                    // Время кэширования preflight
                    policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsOptions.PreflightMaxAge));
                });
            });

            return services;
        }

        /// <summary>
        /// Использует CORS политики
        /// </summary>
        public static IApplicationBuilder UsePitCors(
            this IApplicationBuilder app,
            IOptions<InfrastructureOptions> options)
        {
            var corsOptions = options.Value.Cors;
            app.UseCors(corsOptions.DefaultPolicyName);
            return app;
        }
    }
}
