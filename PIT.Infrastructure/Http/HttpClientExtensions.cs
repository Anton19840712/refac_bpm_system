using Microsoft.Extensions.DependencyInjection;

namespace PIT.Infrastructure.Http
{
    /// <summary>
    /// Расширения для настройки HttpClient с поддержкой Correlation ID
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Добавляет DelegatingHandler для передачи Correlation ID в исходящие запросы
        /// </summary>
        public static IHttpClientBuilder AddCorrelationIdHandler(
            this IHttpClientBuilder builder)
        {
            return builder.AddHttpMessageHandler<CorrelationIdHttpClientHandler>();
        }
    }
}
