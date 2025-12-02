using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PIT.Infrastructure.ErrorHandling.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ErrorHandling
{
    /// <summary>
    /// Глобальный обработчик исключений
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            ProblemDetailsFactory problemDetailsFactory)
        {
            _logger = logger;
            _problemDetailsFactory = problemDetailsFactory;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Необработанное исключение при обработке запроса {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            ProblemDetails problemDetails = exception switch
            {
                // 400 Bad Request
                BadHttpRequestException badRequestEx => _problemDetailsFactory.CreateBadRequestProblemDetails(
                    httpContext,
                    detail: badRequestEx.Message),

                InvalidOperationException invalidOpEx => _problemDetailsFactory.CreateBadRequestProblemDetails(
                    httpContext,
                    detail: invalidOpEx.Message),

                ArgumentException argumentEx => _problemDetailsFactory.CreateBadRequestProblemDetails(
                    httpContext,
                    detail: argumentEx.Message),

                // 401 Unauthorized
                UnauthorizedAccessException => _problemDetailsFactory.CreateUnauthorizedProblemDetails(
                    httpContext,
                    detail: exception.Message),

                // 403 Forbidden
                ForbiddenException => _problemDetailsFactory.CreateForbiddenProblemDetails(
                    httpContext,
                    detail: exception.Message),

                // 404 Not Found
                NotFoundException => _problemDetailsFactory.CreateNotFoundProblemDetails(
                    httpContext,
                    detail: exception.Message),

                KeyNotFoundException => _problemDetailsFactory.CreateNotFoundProblemDetails(
                    httpContext,
                    detail: exception.Message),

                // 408 Request Timeout
                TimeoutException => _problemDetailsFactory.CreateCustomProblemDetails(
                    httpContext,
                    statusCode: StatusCodes.Status408RequestTimeout,
                    type: "request-timeout",
                    title: "Request Timeout",
                    detail: exception.Message),

                // 409 Conflict
                ConflictException => _problemDetailsFactory.CreateConflictProblemDetails(
                    httpContext,
                    detail: exception.Message),

                // 422 Unprocessable Entity
                UnprocessableEntityException unprocessableEx => unprocessableEx.Errors != null
                    ? _problemDetailsFactory.CreateUnprocessableEntityProblemDetails(
                        httpContext,
                        errors: unprocessableEx.Errors,
                        detail: unprocessableEx.Message)
                    : _problemDetailsFactory.CreateUnprocessableEntityProblemDetails(
                        httpContext,
                        detail: unprocessableEx.Message),

                // 499 Client Closed Request
                TaskCanceledException or OperationCanceledException => _problemDetailsFactory.CreateCustomProblemDetails(
                    httpContext,
                    statusCode: StatusCodes.Status499ClientClosedRequest,
                    type: "request-cancelled",
                    title: "Request Cancelled",
                    detail: "Запрос был отменён"),

                // 501 Not Implemented
                NotImplementedException => _problemDetailsFactory.CreateCustomProblemDetails(
                    httpContext,
                    statusCode: StatusCodes.Status501NotImplemented,
                    type: "not-implemented",
                    title: "Not Implemented",
                    detail: exception.Message),

                // 500 Internal Server Error (default)
                _ => _problemDetailsFactory.CreateInternalServerErrorProblemDetails(
                    httpContext,
                    exception: exception)
            };

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }

}
