using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.HealthChecks
{
    /// <summary>
    /// Проверка состояния подключения к базе данных
    /// </summary>
    public sealed class DatabaseHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _databaseType;
        private readonly ILogger<DatabaseHealthCheck> _logger;
        private readonly TimeSpan _timeout;

        public DatabaseHealthCheck(
            string connectionString,
            string databaseType,
            ILogger<DatabaseHealthCheck> logger,
            int timeoutSeconds)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _databaseType = databaseType ?? throw new ArgumentNullException(nameof(databaseType));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeout = TimeSpan.FromSeconds(timeoutSeconds);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var timeoutCts = new CancellationTokenSource(_timeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                await using var connection = CreateConnection();
                await connection.OpenAsync(linkedCts.Token);

                await using var command = connection.CreateCommand();
                command.CommandText = GetHealthCheckQuery();
                command.CommandTimeout = (int)_timeout.TotalSeconds;

                var result = await command.ExecuteScalarAsync(linkedCts.Token);

                stopwatch.Stop();

                var data = new Dictionary<string, object>
                {
                    ["database_type"] = _databaseType,
                    ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                    ["query_result"] = result?.ToString() ?? "null",
                    ["status"] = "healthy"
                };

                _logger.LogInformation(
                    "Проверка работоспособности базы данных успешна. Тип: {DatabaseType}, Время отклика: {ResponseTime} мс",
                    _databaseType,
                    stopwatch.ElapsedMilliseconds);

                return HealthCheckResult.Healthy(
                    "База данных доступна и работает корректно",
                    data);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "Проверка работоспособности базы данных была отменена. Тип: {DatabaseType}",
                    _databaseType);

                return HealthCheckResult.Unhealthy(
                    "Проверка работоспособности базы данных была отменена",
                    data: new Dictionary<string, object>
                    {
                        ["database_type"] = _databaseType,
                        ["status"] = "cancelled"
                    });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "Ошибка при проверке работоспособности базы данных. Тип: {DatabaseType}, Время: {ResponseTime} мс",
                    _databaseType,
                    stopwatch.ElapsedMilliseconds);

                var data = new Dictionary<string, object>
                {
                    ["database_type"] = _databaseType,
                    ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                    ["error_type"] = ex.GetType().Name,
                    ["error_message"] = ex.Message,
                    ["status"] = "unhealthy"
                };

                return HealthCheckResult.Unhealthy(
                    $"Не удалось подключиться к базе данных: {ex.Message}",
                    ex,
                    data);
            }
        }

        private DbConnection CreateConnection()
        {
            return _databaseType.ToLowerInvariant() switch
            {
                "postgresql" or "postgres" or "npgsql" => new NpgsqlConnection(_connectionString),
                _ => throw new NotSupportedException($"Тип базы данных '{_databaseType}' не поддерживается")
            };
        }

        private string GetHealthCheckQuery()
        {
            return _databaseType.ToLowerInvariant() switch
            {
                "postgresql" or "postgres" or "npgsql" => "SELECT 1",
                _ => "SELECT 1"
            };
        }
    }

}
