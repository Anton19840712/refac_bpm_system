using Microsoft.AspNetCore.Http;
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
    /// Обогащает логи Correlation ID из HTTP контекста
    /// </summary>
    public sealed class CorrelationIdEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items.TryGetValue("CorrelationId", out var correlationId) == true)
            {
                var property = propertyFactory.CreateProperty("CorrelationId", correlationId);
                logEvent.AddPropertyIfAbsent(property);
            }
        }
    }
}
