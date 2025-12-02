using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PIT.Infrastructure.ApplicationInfo.Extensions;
using PIT.Infrastructure.Compression;
using PIT.Infrastructure.Configuration;
using PIT.Infrastructure.Cors.Extensions;
using PIT.Infrastructure.HealthChecks.Extensions;
using PIT.Infrastructure.Http;
using PIT.Infrastructure.Logging.Extensions;
using PIT.Infrastructure.OpenAPI;
using PIT.Infrastructure.Security;
using PIT.Infrastructure.Telemetry.Extensions;
using Serilog;

namespace PIT.Infrastructure.Extensions
{
    /// Главные расширения для настройки инфраструктуры
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет всю инфраструктуру PIT в приложение
        /// </summary>
        public static WebApplicationBuilder AddPitInfrastructure(
            this WebApplicationBuilder builder,
            string? configurationSection = null)
        {
            var section = configurationSection ?? InfrastructureOptions.SectionName;
            var configuration = builder.Configuration.GetSection(section);

            builder.Services.Configure<InfrastructureOptions>(configuration);

            var options = configuration.Get<InfrastructureOptions>() ?? new InfrastructureOptions();
            var optionsWrapper = Options.Create(options);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // HTTP Context Accessor (необходим для Correlation ID)
            builder.Services.AddHttpContextAccessor();

            // Swagger
            if (options.EnableSwagger)
            {
                builder.Services.AddOpenApi(optionsWrapper);
            }

            // Error Handling (RFC 9457)
            if (options.EnableErrorHandling)
            {
                builder.Services.AddPitErrorHandling(optionsWrapper);
            }

            // Compression
            if (options.EnableCompression)
            {
                builder.Services.AddPitCompression(optionsWrapper);
            }

            // Logging (Serilog)
            if (options.EnableLogging)
            {
                builder.AddPitLogging();
            }

            // Telemetry (OpenTelemetry)
            if (options.EnableTelemetry)
            {
                builder.Services.AddPitTelemetry(optionsWrapper);
            }

            // CORS
            if (options.EnableCors)
            {
                builder.Services.AddPitCors(optionsWrapper);
            }

            // Health Checks
            if (options.EnableHealthChecks)
            {
                builder.Services.AddPitHealthChecks(optionsWrapper);
            }

            // Application Info
            if (options.EnableApplicationInfo)
            {
                builder.Services.AddPitApplicationInfo();
            }

            // Sentry
            if (options.EnableSentry && !string.IsNullOrWhiteSpace(options.Sentry.Dsn))
            {
                builder.WebHost.UseSentry(sentryOptions =>
                {
                    ConfigureSentry(sentryOptions, options);
                });
            }

            builder.Services.AddScoped<CorrelationIdHttpClientHandler>();

            return builder;
        }

        /// <summary>
        /// Использует всю инфраструктуру PIT в приложении
        /// </summary>
        public static WebApplication UsePitInfrastructure(this WebApplication app)
        {
            var options = app.Services.GetRequiredService<IOptions<InfrastructureOptions>>().Value;

            // Swagger
            if (options.EnableSwagger)
            {
                app.UseOpenApi(Options.Create(options));
            }

            // Security Headers
            if (options.EnableSecurityHeaders)
            {
                app.UsePitSecurityHeaders();
            }

            // Compression 
            if (options.EnableCompression)
            {
                app.UsePitCompression();
            }

            // CORS (должен быть перед авторизацией)
            if (options.EnableCors)
            {
                app.UsePitCors(Options.Create(options));
            }

            // Logging
            if (options.EnableLogging)
            {
                app.UsePitLogging();
            }

            // Error Handling
            if (options.EnableErrorHandling)
            {
                app.UsePitErrorHandling(Options.Create(options));
            }

            // Telemetry
            if (options.EnableTelemetry)
            {
                app.UsePitTelemetry(Options.Create(options));
            }

            // Health Checks
            if (options.EnableHealthChecks)
            {
                app.UsePitHealthChecks(Options.Create(options));
            }

            // Application Info
            if (options.EnableApplicationInfo)
            {
                app.MapPitApplicationInfo();
            }

            app.UseHttpsRedirection();
            app.MapControllers();

            return app;
        }

        private static void ConfigureSentry(Sentry.AspNetCore.SentryAspNetCoreOptions sentryOptions, InfrastructureOptions options)
        {
            var sentryConfig = options.Sentry;

            sentryOptions.Dsn = sentryConfig.Dsn;
            sentryOptions.Environment = sentryConfig.Environment;
            sentryOptions.TracesSampleRate = sentryConfig.TracesSampleRate;
            sentryOptions.SendDefaultPii = sentryConfig.SendDefaultPii;
            sentryOptions.MaxBreadcrumbs = sentryConfig.MaxBreadcrumbs;

            // Профилирование
            if (sentryConfig.EnableProfiling)
            {
                sentryOptions.ProfilesSampleRate = sentryConfig.ProfilesSampleRate;
            }

            // Фильтрация исключений через BeforeSend
            sentryOptions.SetBeforeSend((sentryEvent, hint) =>
            {
                // Получаем исключение из ScopeStackContainer
                var exception = sentryEvent.Exception;

                // Не отправляем OperationCanceledException
                if (exception is OperationCanceledException or TaskCanceledException)
                {
                    return null;
                }

                // Фильтруем по уровню события
                var minimumLevel = sentryConfig.MinimumEventLevel.ToLowerInvariant() switch
                {
                    "debug" => Sentry.SentryLevel.Debug,
                    "info" or "information" => Sentry.SentryLevel.Info,
                    "warning" or "warn" => Sentry.SentryLevel.Warning,
                    "error" => Sentry.SentryLevel.Error,
                    "fatal" or "critical" => Sentry.SentryLevel.Fatal,
                    _ => Sentry.SentryLevel.Warning
                };

                if (sentryEvent.Level.HasValue && sentryEvent.Level.Value < minimumLevel)
                {
                    return null;
                }

                return sentryEvent;
            });

            // Обогащение транзакций
            sentryOptions.SetBeforeSendTransaction((transaction, hint) =>
            {
                transaction.SetTag("service", options.Logging.ServiceName);
                transaction.SetTag("environment", options.Logging.Environment);
                return transaction;
            });

            // Интеграция с Serilog
            if (sentryConfig.EnableSerilogIntegration)
            {
                var loggerConfig = new LoggerConfiguration();

                loggerConfig.WriteTo.Sentry(o =>
                {
                    o.Dsn = sentryConfig.Dsn;
                    o.MinimumBreadcrumbLevel = sentryConfig.MinimumBreadcrumbLevel.ToLowerInvariant() switch
                    {
                        "debug" => Serilog.Events.LogEventLevel.Debug,
                        "info" or "information" => Serilog.Events.LogEventLevel.Information,
                        "warning" or "warn" => Serilog.Events.LogEventLevel.Warning,
                        "error" => Serilog.Events.LogEventLevel.Error,
                        "fatal" or "critical" => Serilog.Events.LogEventLevel.Fatal,
                        _ => Serilog.Events.LogEventLevel.Information
                    };
                    o.MinimumEventLevel = sentryConfig.MinimumEventLevel.ToLowerInvariant() switch
                    {
                        "debug" => Serilog.Events.LogEventLevel.Debug,
                        "info" or "information" => Serilog.Events.LogEventLevel.Information,
                        "warning" or "warn" => Serilog.Events.LogEventLevel.Warning,
                        "error" => Serilog.Events.LogEventLevel.Error,
                        "fatal" or "critical" => Serilog.Events.LogEventLevel.Fatal,
                        _ => Serilog.Events.LogEventLevel.Error
                    };
                });
            }
        }
    }
}
