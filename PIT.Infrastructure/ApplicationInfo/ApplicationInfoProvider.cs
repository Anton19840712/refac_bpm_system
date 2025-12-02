using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using PIT.Infrastructure.ApplicationInfo.Models;
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
    /// Провайдер информации о приложении
    /// </summary>
    public sealed class ApplicationInfoProvider
    {
        private readonly InfrastructureOptions _options;
        private readonly ILogger<ApplicationInfoProvider> _logger;
        private readonly DateTimeOffset _startTime;

        public ApplicationInfoProvider(
            IOptions<InfrastructureOptions> options,
            ILogger<ApplicationInfoProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
            _startTime = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Получает информацию о приложении
        /// </summary>
        public async Task<ApplicationInfoModel> GetApplicationInfoAsync(CancellationToken cancellationToken = default)
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();

            var info = new ApplicationInfoModel
            {
                ApplicationName = _options.Logging.ServiceName,
                Version = assemblyName.Version?.ToString() ?? "1.0.0",
                Environment = _options.Logging.Environment,
                HostName = Environment.MachineName,
                DotNetVersion = RuntimeInformation.FrameworkDescription,
                OperatingSystem = RuntimeInformation.OSDescription,
                ProcessorArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                StartTime = _startTime,
                Uptime = DateTimeOffset.UtcNow - _startTime                
            };

            // Получаем информацию о базе данных
            if (_options.HealthCheck.EnableDatabaseCheck &&
                !string.IsNullOrWhiteSpace(_options.HealthCheck.DatabaseConnectionString))
            {
                info.Database = await GetDatabaseInfoAsync(cancellationToken);
            }

            return info;
        }

        private async Task<DatabaseInfo?> GetDatabaseInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                var connectionString = _options.HealthCheck.DatabaseConnectionString;
                var databaseType = _options.HealthCheck.DatabaseType;

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return null;
                }

                return databaseType.ToLowerInvariant() switch
                {
                    "postgresql" or "postgres" or "npgsql" => await GetPostgreSqlInfoAsync(connectionString, cancellationToken),
                    _ => new DatabaseInfo { Type = databaseType }
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось получить информацию о базе данных");
                return null;
            }
        }

        private async Task<DatabaseInfo> GetPostgreSqlInfoAsync(string connectionString, CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT version()";
            var version = (await command.ExecuteScalarAsync(cancellationToken))?.ToString();

            var builder = new NpgsqlConnectionStringBuilder(connectionString);

            return new DatabaseInfo
            {
                Type = "PostgreSQL",
                Version = version,
                Host = builder.Host,
                Port = builder.Port,
                DatabaseName = builder.Database
            };
        }
    }
}
