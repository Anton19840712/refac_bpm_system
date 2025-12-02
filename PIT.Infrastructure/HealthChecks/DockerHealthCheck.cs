using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.HealthChecks
{
    /// <summary>
    /// Специальная проверка для Docker/Kubernetes
    /// </summary>
    public sealed class DockerHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            // Простая проверка что приложение запущено
            // Используется для Docker HEALTHCHECK и Kubernetes startup probe
            return Task.FromResult(HealthCheckResult.Healthy("Приложение запущено"));
        }
    }
}
