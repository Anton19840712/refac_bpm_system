using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PIT.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Security
{
    /// <summary>
    /// Middleware для добавления Security Headers
    /// </summary>
    public sealed class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SecurityHeadersOptions _options;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(
            RequestDelegate next,
            IOptions<InfrastructureOptions> options,
            ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _options = options.Value.SecurityHeaders;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogDebug("Adding security headers to response");

            var response = context.Response;

            // Удаляем заголовки сервера
            response.Headers.Remove("Server");
            response.Headers.Remove("X-Powered-By");
            response.Headers.Remove("X-AspNet-Version");
            response.Headers.Remove("X-AspNetMvc-Version");

            // Добавляем security заголовки
            if (!string.IsNullOrEmpty(_options.XFrameOptions))
                response.Headers.Add("X-Frame-Options", _options.XFrameOptions);

            if (!string.IsNullOrEmpty(_options.XContentTypeOptions))
                response.Headers.Add("X-Content-Type-Options", _options.XContentTypeOptions);

            if (!string.IsNullOrEmpty(_options.XXssProtection))
                response.Headers.Add("X-XSS-Protection", _options.XXssProtection);

            if (!string.IsNullOrEmpty(_options.ReferrerPolicy))
                response.Headers.Add("Referrer-Policy", _options.ReferrerPolicy);

            if (!string.IsNullOrEmpty(_options.PermissionsPolicy))
                response.Headers.Add("Permissions-Policy", _options.PermissionsPolicy);

            if (!string.IsNullOrEmpty(_options.ContentSecurityPolicy))
                response.Headers.Add("Content-Security-Policy", _options.ContentSecurityPolicy);

            // HSTS только для HTTPS
            if (!string.IsNullOrEmpty(_options.StrictTransportSecurity))
            {
                if (_options.EnableHstsOnlyForHttps)
                {
                    if (context.Request.IsHttps)
                        response.Headers.Add("Strict-Transport-Security", _options.StrictTransportSecurity);
                }
                else
                {
                    response.Headers.Add("Strict-Transport-Security", _options.StrictTransportSecurity);
                }
            }

            _logger.LogDebug("Security headers added successfully");

            await _next(context);
        }
    }
}
