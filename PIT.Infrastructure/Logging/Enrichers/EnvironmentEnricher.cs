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
    /// Обогащает логи информацией об окружении
    /// </summary>
    public sealed class EnvironmentEnricher : ILogEventEnricher
    {
        private readonly string _environment;

        public EnvironmentEnricher(string environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var property = propertyFactory.CreateProperty("Environment", _environment);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
