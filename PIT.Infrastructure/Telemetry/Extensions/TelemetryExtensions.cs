using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PIT.Infrastructure.Configuration;
using PIT.Infrastructure.Telemetry.Metrics;

namespace PIT.Infrastructure.Telemetry.Extensions
{
    /// <summary>
    /// Расширения для настройки OpenTelemetry
    /// </summary>
    public static class TelemetryExtensions
    {
        /// <summary>
        /// Добавляет OpenTelemetry метрики и трассировку
        /// </summary>
        public static IServiceCollection AddPitTelemetry(
            this IServiceCollection services,
            IOptions<InfrastructureOptions> options)
        {
            var telemetryOptions = options.Value.Telemetry;

            // Создаём ресурс с информацией о сервисе
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(
                    serviceName: telemetryOptions.ServiceName,
                    serviceVersion: telemetryOptions.ServiceVersion,
                    serviceNamespace: telemetryOptions.ServiceNamespace)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = options.Value.Logging.Environment,
                    ["host.name"] = Environment.MachineName
                });

            // Добавляем OpenTelemetry
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(
                    serviceName: telemetryOptions.ServiceName,
                    serviceVersion: telemetryOptions.ServiceVersion))
                .WithMetrics(metrics =>
                {
                    metrics.SetResourceBuilder(resourceBuilder);

                    // Добавляем стандартные инструментации
                    if (telemetryOptions.EnableAspNetCoreInstrumentation)
                    {
                        metrics.AddAspNetCoreInstrumentation();
                    }

                    if (telemetryOptions.EnableHttpClientInstrumentation)
                    {
                        metrics.AddHttpClientInstrumentation();
                    }

                    if (telemetryOptions.EnableRuntimeInstrumentation)
                    {
                        metrics.AddRuntimeInstrumentation();
                    }

                    // Добавляем кастомные метрики
                    if (telemetryOptions.EnableCustomMetrics)
                    {
                        metrics.AddMeter(telemetryOptions.ServiceName);
                    }

                    // Добавляем экспортеры
                    if (telemetryOptions.EnableConsoleExporter)
                    {
                        metrics.AddConsoleExporter();
                    }

                    if (telemetryOptions.EnableOtlpExporter)
                    {
                        metrics.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(telemetryOptions.OtlpEndpoint);
                            otlpOptions.Protocol = telemetryOptions.OtlpProtocol.ToLowerInvariant() switch
                            {
                                "grpc" => OpenTelemetry.Exporter.OtlpExportProtocol.Grpc,
                                "httpprotobuf" => OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf,
                                _ => OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
                            };
                        });
                    }
                })
                .WithTracing(tracing =>
                {
                    tracing.SetResourceBuilder(resourceBuilder);

                    // Добавляем стандартные инструментации
                    if (telemetryOptions.EnableAspNetCoreInstrumentation)
                    {
                        tracing.AddAspNetCoreInstrumentation(aspNetCoreOptions =>
                        {
                            aspNetCoreOptions.RecordException = true;
                            aspNetCoreOptions.Filter = httpContext =>
                            {
                                // Не трассируем health check endpoints
                                var path = httpContext.Request.Path.Value;
                                return path is null || !path.StartsWith("/health");
                            };
                        });
                    }

                    if (telemetryOptions.EnableHttpClientInstrumentation)
                    {
                        tracing.AddHttpClientInstrumentation(httpClientOptions =>
                        {
                            httpClientOptions.RecordException = true;
                        });
                    }

                    // Добавляем экспортеры
                    if (telemetryOptions.EnableConsoleExporter)
                    {
                        tracing.AddConsoleExporter();
                    }

                    if (telemetryOptions.EnableOtlpExporter)
                    {
                        tracing.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(telemetryOptions.OtlpEndpoint);
                            otlpOptions.Protocol = telemetryOptions.OtlpProtocol.ToLowerInvariant() switch
                            {
                                "grpc" => OpenTelemetry.Exporter.OtlpExportProtocol.Grpc,
                                "httpprotobuf" => OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf,
                                _ => OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
                            };
                        });
                    }
                });

            // Регистрируем кастомные метрики
            if (telemetryOptions.EnableCustomMetrics)
            {
                services.AddSingleton(sp => new ApplicationMetrics(
                    telemetryOptions.ServiceName,
                    telemetryOptions.ServiceVersion));
            }

            return services;
        }

        /// <summary>
        /// Использует middleware для сбора метрик
        /// </summary>
        public static IApplicationBuilder UsePitTelemetry(
            this IApplicationBuilder app,
            IOptions<InfrastructureOptions> options)
        {
            var telemetryOptions = options.Value.Telemetry;

            if (!telemetryOptions.EnableCustomMetrics)
            {
                return app;
            }

            app.Use(async (context, next) =>
            {
                var metrics = context.RequestServices.GetService<ApplicationMetrics>();
                if (metrics is null)
                {
                    await next();
                    return;
                }

                metrics.IncrementActiveRequests();
                var startTime = DateTime.UtcNow;

                try
                {
                    await next();

                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    var statusCode = context.Response.StatusCode;

                    metrics.RecordRequestDuration(
                        duration,
                        context.Request.Method,
                        context.Request.Path,
                        statusCode);

                    metrics.IncrementRequestCount(
                        context.Request.Method,
                        context.Request.Path,
                        statusCode);
                }
                catch (Exception ex)
                {
                    metrics.IncrementErrorCount(
                        ex.GetType().Name,
                        ex.Message);
                    throw;
                }
                finally
                {
                    metrics.DecrementActiveRequests();
                }
            });

            return app;
        }
    }
}
