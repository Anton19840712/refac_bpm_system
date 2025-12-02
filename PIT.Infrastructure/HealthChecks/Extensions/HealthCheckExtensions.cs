using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PIT.Infrastructure.Configuration;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthCheckOptions = Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions;
using Microsoft.Extensions.Configuration;

namespace PIT.Infrastructure.HealthChecks.Extensions
{
    /// <summary>
    /// Расширения для настройки Health Checks
    /// </summary>
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Добавляет Health Checks
        /// </summary>
        public static IServiceCollection AddPitHealthChecks(
            this IServiceCollection services,
            IOptions<InfrastructureOptions> options)
        {
            // Получить IConfiguration из DI
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var healthCheckOptions = options.Value.HealthCheck;

            var healthChecksBuilder = services.AddHealthChecks();

            healthChecksBuilder.AddCheck<DockerHealthCheck>(
                "startup",
                tags: new[] { "startup", "liveness" });

            healthChecksBuilder.AddCheck("self", () => HealthCheckResult.Healthy("Приложение запущено и работает"),
                tags: new[] { "self", "liveness" });

            // Fallback на ConnectionStrings.DefaultConnection
            var connectionString = healthCheckOptions.DatabaseConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = configuration.GetConnectionString("DefaultConnection");
            }

            if (healthCheckOptions.EnableDatabaseCheck && !string.IsNullOrWhiteSpace(connectionString))
            {
                healthChecksBuilder.Add(new HealthCheckRegistration(
                    "database",
                    sp => new DatabaseHealthCheck(
                        connectionString,
                        healthCheckOptions.DatabaseType,
                        sp.GetRequiredService<ILogger<DatabaseHealthCheck>>(),
                        healthCheckOptions.TimeoutSeconds),
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database", "readiness" }));
            }

            return services;
        }

        /// <summary>
        /// Использует Health Check endpoints
        /// </summary>
        public static IApplicationBuilder UsePitHealthChecks(
            this IApplicationBuilder app,
            IOptions<InfrastructureOptions> options)
        {
            var healthCheckOptions = options.Value.HealthCheck;

            // Общий health check endpoint
            app.UseHealthChecks(healthCheckOptions.Endpoint, new HealthCheckOptions
            {
                ResponseWriter = healthCheckOptions.EnableDetailedOutput
                    ? WriteDetailedHealthCheckResponse
                    : WriteSimpleHealthCheckResponse
            });

            // Readiness endpoint (проверяет готовность к обработке запросов)
            app.UseHealthChecks(healthCheckOptions.ReadinessEndpoint, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("readiness"),
                ResponseWriter = healthCheckOptions.EnableDetailedOutput
                    ? WriteDetailedHealthCheckResponse
                    : WriteSimpleHealthCheckResponse
            });

            // Liveness endpoint (проверяет что приложение живо)
            app.UseHealthChecks(healthCheckOptions.LivenessEndpoint, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("liveness"),
                ResponseWriter = WriteSimpleHealthCheckResponse
            });

            // Startup endpoint (проверяет что приложение стартовало)
            // Используется в Kubernetes startupProbe и Docker HEALTHCHECK
            app.UseHealthChecks("/health/startup", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("startup") || check.Tags.Contains("liveness"),
                ResponseWriter = WriteSimpleHealthCheckResponse
            });


            return app;
        }

        private static Task WriteSimpleHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTimeOffset.UtcNow
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            return context.Response.WriteAsync(result);
        }

        private static Task WriteDetailedHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTimeOffset.UtcNow,
                duration = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration.TotalMilliseconds,
                    exception = entry.Value.Exception?.Message,
                    data = entry.Value.Data,
                    tags = entry.Value.Tags
                })
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            return context.Response.WriteAsync(result);
        }
    }


}
