using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Logging.Enrichers
{
    /// <summary>
    /// Обогащает логи именем сервиса
    /// </summary>
    public sealed class ServiceNameEnricher : ILogEventEnricher
    {
        private readonly string _serviceName;

        public ServiceNameEnricher(string serviceName)
        {
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var property = propertyFactory.CreateProperty("ServiceName", _serviceName);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
