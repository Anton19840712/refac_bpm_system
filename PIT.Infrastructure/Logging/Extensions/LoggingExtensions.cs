using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PIT.Infrastructure.Configuration;
using PIT.Infrastructure.Logging.Enrichers;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.SystemConsole.Themes;

namespace PIT.Infrastructure.Logging.Extensions
{
    /// <summary>
    /// Расширения для настройки структурированного логирования Serilog
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Добавляет Serilog в приложение
        /// </summary>
        public static IHostBuilder AddPitLogging(
            this IHostBuilder hostBuilder,
            IOptions<InfrastructureOptions> options)
        {
            var loggingOptions = options.Value.Logging;

            hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            {
                ConfigureSerilog(loggerConfiguration, loggingOptions, services, context.HostingEnvironment);
            });

            return hostBuilder;
        }

        /// <summary>
        /// Добавляет Serilog в WebApplicationBuilder
        /// </summary>
        public static WebApplicationBuilder AddPitLogging(
            this WebApplicationBuilder builder)
        {
            var options = builder.Configuration
                .GetSection(InfrastructureOptions.SectionName)
                .Get<InfrastructureOptions>() ?? new InfrastructureOptions();

            var loggingOptions = options.Logging;

            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                ConfigureSerilog(loggerConfiguration, loggingOptions, services, context.HostingEnvironment);
            });

            return builder;
        }

        private static void ConfigureSerilog(
            LoggerConfiguration loggerConfiguration,
            LoggingOptions loggingOptions,
            IServiceProvider services,
            IHostEnvironment environment)
        {
            // Базовая конфигурация
            loggerConfiguration
                .MinimumLevel.Is(ParseLogLevel(loggingOptions.MinimumLevel))
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning);

            // Обогащение логов
            loggerConfiguration.Enrich.FromLogContext();

            if (loggingOptions.EnrichWithEnvironment)
            {
                loggerConfiguration
                    .Enrich.WithMachineName()
                    .Enrich.With(new EnvironmentEnricher(loggingOptions.Environment));
            }

            if (loggingOptions.EnrichWithThread)
            {
                loggerConfiguration.Enrich.WithThreadId();
            }

            if (loggingOptions.EnrichWithProcess)
            {
                loggerConfiguration
                    .Enrich.WithProcessId()
                    .Enrich.WithProcessName();
            }

            // Обогащение именем сервиса
            loggerConfiguration.Enrich.With(new ServiceNameEnricher(loggingOptions.ServiceName));

            // Обогащение Correlation ID
            if (loggingOptions.EnrichWithCorrelationId)
            {
                var httpContextAccessor = services.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
                if (httpContextAccessor is not null)
                {
                    loggerConfiguration.Enrich.With(new CorrelationIdEnricher(httpContextAccessor));
                }
            }

            // Консольный вывод
            if (loggingOptions.EnableConsole)
            {
                ConfigureConsoleSink(loggerConfiguration, loggingOptions, environment);
            }

            // Файловый вывод
            if (loggingOptions.EnableFile && !string.IsNullOrWhiteSpace(loggingOptions.FilePath))
            {
                ConfigureFileSink(loggerConfiguration, loggingOptions);
            }
        }

        private static void ConfigureConsoleSink(
            LoggerConfiguration loggerConfiguration,
            LoggingOptions loggingOptions,
            IHostEnvironment environment)
        {
            // Для Development - красивый читаемый формат с цветами
            if (environment.IsDevelopment())
            {
                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code);
            }
            else
            {
                // Для Production - по настройкам
                switch (loggingOptions.OutputFormat.ToLowerInvariant())
                {
                    case "json":
                        loggerConfiguration.WriteTo.Console(new JsonFormatter());
                        break;

                    case "compact":
                        loggerConfiguration.WriteTo.Console(new CompactJsonFormatter());
                        break;

                    case "text":
                    default:
                        loggerConfiguration.WriteTo.Console(
                            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}",
                            theme: SystemConsoleTheme.Literate);
                        break;
                }
            }
        }

        private static void ConfigureFileSink(
            LoggerConfiguration loggerConfiguration,
            LoggingOptions loggingOptions)
        {
            var rollingInterval = ParseRollingInterval(loggingOptions.RollingInterval);

            switch (loggingOptions.OutputFormat.ToLowerInvariant())
            {
                case "json":
                    loggerConfiguration.WriteTo.File(
                        new JsonFormatter(),
                        loggingOptions.FilePath!,
                        rollingInterval: rollingInterval,
                        retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1));
                    break;

                case "compact":
                    loggerConfiguration.WriteTo.File(
                        new CompactJsonFormatter(),
                        loggingOptions.FilePath!,
                        rollingInterval: rollingInterval,
                        retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1));
                    break;

                case "text":
                default:
                    loggerConfiguration.WriteTo.File(
                        loggingOptions.FilePath!,
                        rollingInterval: rollingInterval,
                        retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1),
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}");
                    break;
            }
        }

        private static LogEventLevel ParseLogLevel(string level)
        {
            return level.ToLowerInvariant() switch
            {
                "verbose" or "trace" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" or "info" => LogEventLevel.Information,
                "warning" or "warn" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" or "critical" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }

        private static RollingInterval ParseRollingInterval(string interval)
        {
            return interval.ToLowerInvariant() switch
            {
                "infinite" => RollingInterval.Infinite,
                "year" => RollingInterval.Year,
                "month" => RollingInterval.Month,
                "day" => RollingInterval.Day,
                "hour" => RollingInterval.Hour,
                "minute" => RollingInterval.Minute,
                _ => RollingInterval.Day
            };
        }

        /// <summary>
        /// Использует Serilog для логирования HTTP запросов
        /// </summary>
        public static IApplicationBuilder UsePitLogging(this IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} ответил {StatusCode} за {Elapsed:0.0000} мс";

                options.GetLevel = (httpContext, elapsed, ex) =>
                {
                    if (ex != null)
                        return LogEventLevel.Error;

                    if (httpContext.Response.StatusCode >= 500)
                        return LogEventLevel.Error;

                    if (httpContext.Response.StatusCode >= 400)
                        return LogEventLevel.Warning;

                    return LogEventLevel.Information;
                };

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                    diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());

                    if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
                    {
                        diagnosticContext.Set("CorrelationId", correlationId);
                    }

                    if (httpContext.User?.Identity?.IsAuthenticated == true)
                    {
                        diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
                    }
                };
            });

            return app;
        }
    }
}
