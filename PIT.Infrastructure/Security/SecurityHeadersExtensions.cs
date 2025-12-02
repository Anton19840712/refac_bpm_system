using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Security
{
    /// <summary>
    /// Расширения для Security Headers
    /// </summary>
    public static class SecurityHeadersExtensions
    {
        /// <summary>
        /// Использует Security Headers middleware
        /// </summary>
        public static IApplicationBuilder UsePitSecurityHeaders(this IApplicationBuilder app)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
            return app;
        }
    }
}
