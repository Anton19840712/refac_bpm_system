using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Http
{
    /// <summary>
    /// DelegatingHandler для автоматической передачи Correlation ID в исходящие HTTP запросы
    /// </summary>
    public sealed class CorrelationIdHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdHttpClientHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var correlationId = GetCorrelationId();

            if (!string.IsNullOrEmpty(correlationId))
            {
                request.Headers.Add("X-Correlation-ID", correlationId);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private string? GetCorrelationId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            // Сначала проверяем Request headers
            if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue))
            {
                return headerValue.ToString();
            }

            // Затем проверяем Items (если добавлено вручную)
            if (httpContext.Items.TryGetValue("CorrelationId", out var itemValue))
            {
                return itemValue?.ToString();
            }

            return null;
        }
    }
}
