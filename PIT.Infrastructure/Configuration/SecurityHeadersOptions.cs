namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки Security Headers
    /// </summary>
    public sealed class SecurityHeadersOptions
    {
        /// <summary>
        /// X-Frame-Options (защита от Clickjacking)
        /// </summary>
        public string XFrameOptions { get; set; } = "DENY";

        /// <summary>
        /// X-Content-Type-Options (защита от MIME sniffing)
        /// </summary>
        public string XContentTypeOptions { get; set; } = "nosniff";

        /// <summary>
        /// X-XSS-Protection (защита от XSS)
        /// </summary>
        public string XXssProtection { get; set; } = "1; mode=block";

        /// <summary>
        /// Referrer-Policy
        /// </summary>
        public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

        /// <summary>
        /// Permissions-Policy
        /// </summary>
        public string PermissionsPolicy { get; set; } = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

        /// <summary>
        /// Content-Security-Policy
        /// </summary>
        public string? ContentSecurityPolicy { get; set; } = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';";

        /// <summary>
        /// Strict-Transport-Security (HSTS)
        /// </summary>
        public string? StrictTransportSecurity { get; set; } = "max-age=31536000; includeSubDomains";

        /// <summary>
        /// Удалить заголовок Server
        /// </summary>
        public bool RemoveServerHeader { get; set; } = true;

        /// <summary>
        /// Удалить заголовок X-Powered-By
        /// </summary>
        public bool RemoveXPoweredByHeader { get; set; } = true;

        /// <summary>
        /// Включить HSTS только для HTTPS
        /// </summary>
        public bool EnableHstsOnlyForHttps { get; set; } = true;
    }

}
