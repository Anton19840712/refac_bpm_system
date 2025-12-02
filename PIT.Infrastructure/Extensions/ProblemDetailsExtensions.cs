using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PIT.Infrastructure.Configuration;
using PIT.Infrastructure.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.Extensions
{
    /// <summary>
    /// Расширения для настройки Problem Details
    /// </summary>
    public static class ProblemDetailsExtensions
    {
        /// <summary>
        /// Добавляет обработку ошибок по RFC 9457
        /// </summary>
        public static IServiceCollection AddPitErrorHandling(
            this IServiceCollection services,
            IOptions<InfrastructureOptions> options)
        {
            var errorOptions = options.Value.ErrorHandling;

            // Регистрируем фабрику
            services.AddSingleton(sp => new ProblemDetailsFactory(
                errorOptions.ProblemDetailsBaseUrl,
                errorOptions.IncludeExceptionDetails));

            // Регистрируем глобальный обработчик исключений
            services.AddExceptionHandler<GlobalExceptionHandler>();

            // Настраиваем Problem Details
            services.AddProblemDetails(problemDetailsOptions =>
            {
                problemDetailsOptions.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                    if (context.HttpContext.Items.TryGetValue("CorrelationId", out var correlationId))
                    {
                        context.ProblemDetails.Extensions["correlationId"] = correlationId;
                    }

                    context.ProblemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                };
            });

            // FluentValidation интеграция
            if (errorOptions.EnableFluentValidation)
            {
                services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            }

            return services;
        }

        /// <summary>
        /// Использует обработку ошибок по RFC 9457
        /// </summary>
        public static IApplicationBuilder UsePitErrorHandling(
            this IApplicationBuilder app,
            IOptions<InfrastructureOptions> options)
        {
            var errorOptions = options.Value.ErrorHandling;

            // Middleware для Correlation ID
            app.Use(async (context, next) =>
            {
                var correlationId = context.Request.Headers[errorOptions.CorrelationIdHeader].FirstOrDefault()
                    ?? Guid.NewGuid().ToString();

                context.Items["CorrelationId"] = correlationId;
                context.Response.Headers.Append(errorOptions.CorrelationIdHeader, correlationId);

                await next();
            });

            // Глобальная обработка исключений
            app.UseExceptionHandler();

            // Обработка статусных кодов без исключений
            app.UseStatusCodePages(async context =>
            {
                var httpContext = context.HttpContext;
                var statusCode = httpContext.Response.StatusCode;

                if (statusCode >= 400 && statusCode < 600 && !httpContext.Response.HasStarted)
                {
                    var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                    var problemDetails = statusCode switch
                    {
                        StatusCodes.Status400BadRequest => factory.CreateBadRequestProblemDetails(httpContext),
                        StatusCodes.Status401Unauthorized => factory.CreateUnauthorizedProblemDetails(httpContext),
                        StatusCodes.Status403Forbidden => factory.CreateForbiddenProblemDetails(httpContext),
                        StatusCodes.Status404NotFound => factory.CreateNotFoundProblemDetails(httpContext),
                        StatusCodes.Status405MethodNotAllowed => factory.CreateMethodNotAllowedProblemDetails(httpContext),
                        StatusCodes.Status500InternalServerError => factory.CreateInternalServerErrorProblemDetails(httpContext),
                        _ => factory.CreateCustomProblemDetails(
                            httpContext,
                            statusCode,
                            $"error-{statusCode}",
                            $"HTTP {statusCode}",
                            $"An error occurred with status code {statusCode}.")
                    };

                    httpContext.Response.ContentType = "application/problem+json";
                    await httpContext.Response.WriteAsJsonAsync(problemDetails);
                }
            });

            return app;
        }
    }
}
