using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PIT.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ApplicationInfo
{
    /// <summary>
    /// Сервис для отображения баннера при запуске приложения
    /// </summary>
    public sealed class StartupBanner : IHostedService
    {
        private readonly ILogger<StartupBanner> _logger;
        private readonly InfrastructureOptions _options;
        private readonly IHostEnvironment _environment;

        public StartupBanner(
            ILogger<StartupBanner> logger,
            IOptions<InfrastructureOptions> options,
            IHostEnvironment environment)
        {
            _logger = logger;
            _options = options.Value;
            _environment = environment;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Устанавливаем UTF-8 для Windows консоли
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.OutputEncoding = Encoding.UTF8;
            }

            PrintBanner();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("================================================================================");
            _logger.LogInformation("Приложение {ServiceName} остановлено", _options.Logging.ServiceName);
            _logger.LogInformation("================================================================================");
            return Task.CompletedTask;
        }

        private void PrintBanner()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
            var frameworkVersion = RuntimeInformation.FrameworkDescription;
            var osVersion = RuntimeInformation.OSDescription;
            var architecture = RuntimeInformation.ProcessArchitecture;

            var banner = new StringBuilder();
            banner.AppendLine();
            banner.AppendLine("================================================================================");
            banner.AppendLine();
            banner.AppendLine($"  Сервис:           {_options.Logging.ServiceName}");
            banner.AppendLine($"  Версия:           {version}");
            banner.AppendLine($"  Окружение:        {_environment.EnvironmentName}");
            banner.AppendLine($"  Хост:             {Environment.MachineName}");
            banner.AppendLine($"  .NET:             {frameworkVersion}");
            banner.AppendLine($"  ОС:               {osVersion}");
            banner.AppendLine($"  Архитектура:      {architecture}");
            banner.AppendLine($"  Производитель:    © {DateTime.Now.Year} ПРОТЕЙ Ай-Ти-Инжиниринг");
            banner.AppendLine();
            banner.AppendLine("================================================================================");
            banner.AppendLine();
            banner.AppendLine("  Включенные модули:");
            banner.AppendLine();

            if (_options.EnableErrorHandling)
                banner.AppendLine("    [+] Error Handling (RFC 9457)");

            if (_options.EnableCompression)
                banner.AppendLine($"    [+] Response Compression (Brotli: {_options.Compression.EnableBrotli}, Gzip: {_options.Compression.EnableGzip})");

            if (_options.EnableLogging)
                banner.AppendLine($"    [+] Structured Logging (Serilog, Level: {_options.Logging.MinimumLevel})");

            if (_options.EnableTelemetry)
                banner.AppendLine($"    [+] Telemetry (OpenTelemetry, OTLP: {_options.Telemetry.EnableOtlpExporter})");

            if (_options.EnableCors)
                banner.AppendLine($"    [+] CORS (Policy: {_options.Cors.DefaultPolicyName})");

            if (_options.EnableHealthChecks)
            {
                var endpoints = new List<string> { _options.HealthCheck.Endpoint };
                if (_options.HealthCheck.EnableDatabaseCheck)
                    endpoints.Add("+ Database");
                banner.AppendLine($"    [+] Health Checks ({string.Join(", ", endpoints)})");
            }

            if (_options.EnableApplicationInfo)
                banner.AppendLine("    [+] Application Info (/info)");

            if (_options.EnableSentry && !string.IsNullOrWhiteSpace(_options.Sentry.Dsn))
                banner.AppendLine($"    [+] Sentry Monitoring (Env: {_options.Sentry.Environment})");

            if (_options.EnableSecurityHeaders)
                banner.AppendLine("    [+] Security Headers");

            banner.AppendLine();
            banner.AppendLine("================================================================================");
            banner.AppendLine();

            if (_options.HealthCheck.EnableDatabaseCheck && !string.IsNullOrWhiteSpace(_options.HealthCheck.DatabaseConnectionString))
            {
                var dbInfo = ParseConnectionString(_options.HealthCheck.DatabaseConnectionString);
                banner.AppendLine("  База данных:");
                banner.AppendLine($"    Тип:            {_options.HealthCheck.DatabaseType}");
                if (dbInfo.Host != null)
                    banner.AppendLine($"    Хост:           {dbInfo.Host}:{dbInfo.Port}");
                if (dbInfo.Database != null)
                    banner.AppendLine($"    БД:             {dbInfo.Database}");
                banner.AppendLine();
                banner.AppendLine("================================================================================");
                banner.AppendLine();
            }

            banner.AppendLine("  >> Приложение успешно запущено!");
            banner.AppendLine();
            banner.AppendLine("================================================================================");

            _logger.LogInformation("{Banner}", banner.ToString());
        }

        private (string? Host, int? Port, string? Database) ParseConnectionString(string connectionString)
        {
            try
            {
                var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                string? host = null;
                int? port = null;
                string? database = null;

                foreach (var part in parts)
                {
                    var keyValue = part.Split('=', 2);
                    if (keyValue.Length != 2) continue;

                    var key = keyValue[0].Trim().ToLowerInvariant();
                    var value = keyValue[1].Trim();

                    switch (key)
                    {
                        case "host" or "server" or "data source":
                            host = value;
                            break;
                        case "port":
                            if (int.TryParse(value, out var p))
                                port = p;
                            break;
                        case "database" or "initial catalog":
                            database = value;
                            break;
                    }
                }

                return (host, port, database);
            }
            catch
            {
                return (null, null, null);
            }
        }
    }
}
