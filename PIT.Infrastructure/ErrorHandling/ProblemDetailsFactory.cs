using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ErrorHandling
{
    /// <summary>
    /// Фабрика для создания Problem Details согласно RFC 9457
    /// </summary>
    public sealed class ProblemDetailsFactory
    {
        private readonly string _baseUrl;
        private readonly bool _includeExceptionDetails;

        public ProblemDetailsFactory(string baseUrl, bool includeExceptionDetails = false)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _includeExceptionDetails = includeExceptionDetails;
        }

        /// <summary>
        /// Создаёт Problem Details для 400 Bad Request
        /// </summary>
        public ProblemDetails CreateBadRequestProblemDetails(
            HttpContext httpContext,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = $"{_baseUrl}/bad-request",
                Title = "Bad Request",
                Detail = detail ?? "The request could not be understood by the server due to malformed syntax.",
                Instance = instance ?? httpContext.Request.Path
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }

        /// <summary>
        /// Создаёт Validation Problem Details для 400 Bad Request с ошибками валидации
        /// </summary>
        public ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            IDictionary<string, string[]> errors,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Type = $"{_baseUrl}/validation-error",
                Title = "One or more validation errors occurred.",
                Detail = detail,
                Instance = instance ?? httpContext.Request.Path
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }

        /// <summary>
        /// Создаёт Problem Details для 401 Unauthorized
        /// </summary>
        public ProblemDetails CreateUnauthorizedProblemDetails(
            HttpContext httpContext,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = $"{_baseUrl}/unauthorized",
                Title = "Unauthorized",
                Detail = detail ?? "Authentication is required to access this resource.",
                Instance = instance ?? httpContext.Request.Path
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }

        /// <summary>
        /// Создаёт Problem Details для 403 Forbidden
        /// </summary>
        public ProblemDetails CreateForbiddenProblemDetails(
            HttpContext httpContext,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = $"{_baseUrl}/forbidden",
                Title = "Forbidden",
                Detail = detail ?? "You do not have permission to access this resource.",
                Instance = instance ?? httpContext.Request.Path
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }

        /// <summary>
        /// Создаёт Problem Details для 404 Not Found
        /// </summary>
        public ProblemDetails CreateNotFoundProblemDetails(
            HttpContext httpContext,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = $"{_baseUrl}/not-found",
                Title = "Not Found",
                Detail = detail ?? "The requested resource was not found.",
                Instance = instance ?? httpContext.Request.Path
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }

        /// <summary>
        /// Создаёт Problem Details для 405 Method Not Allowed
        /// </summary>
        public ProblemDetails CreateMethodNotAllowedProblemDetails(
            HttpContext httpContext,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status405MethodNotAllowed,
                Type = $"{_baseUrl}/method-not-allowed",
                Title = "Method Not Allowed",
                Detail = detail ?? "The requested method is not allowed for this resource.",
                Instance = instance ?? httpContext.Request.Path
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }

        /// <summary>
        /// Создаёт Problem Details для 500 Internal Server Error
        /// </summary>
        public ProblemDetails CreateInternalServerErrorProblemDetails(
            HttpContext httpContext,
            Exception? exception = null,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = $"{_baseUrl}/internal-server-error",
                Title = "Internal Server Error",
                Detail = detail ?? "An unexpected error occurred while processing your request.",
                Instance = instance ?? httpContext.Request.Path
            };

            if (_includeExceptionDetails && exception is not null)
            {
                extensions ??= new Dictionary<string, object?>();
                extensions["exception"] = new
                {
                    type = exception.GetType().FullName,
                    message = exception.Message,
                    stackTrace = exception.StackTrace,
                    innerException = exception.InnerException?.Message
                };
            }

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }

        /// <summary>
        /// Создаёт кастомный Problem Details
        /// </summary>
        public ProblemDetails CreateCustomProblemDetails(
            HttpContext httpContext,
            int statusCode,
            string type,
            string title,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Type = type.StartsWith("http") ? type : $"{_baseUrl}/{type}",
                Title = title,
                Detail = detail,
                Instance = instance ?? httpContext.Request.Path
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }

        /// <summary>
        /// Создаёт Problem Details для 409 Conflict
        /// </summary>
        public ProblemDetails CreateConflictProblemDetails(
            HttpContext httpContext,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type = $"{_baseUrl}/conflict",
                Title = "Conflict",
                Detail = detail ?? "Запрос конфликтует с текущим состоянием ресурса.",
                Instance = instance ?? httpContext.Request.Path
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }

        /// <summary>
        /// Создаёт Problem Details для 422 Unprocessable Entity
        /// </summary>
        public ValidationProblemDetails CreateUnprocessableEntityProblemDetails(
            HttpContext httpContext,
            IDictionary<string, string[]>? errors = null,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ValidationProblemDetails(errors ?? new Dictionary<string, string[]>())
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Type = $"{_baseUrl}/unprocessable-entity",
                Title = "Unprocessable Entity",
                Detail = detail ?? "Запрос синтаксически корректен, но содержит семантические ошибки.",
                Instance = instance ?? httpContext.Request.Path
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, extensions);
            return problemDetails;
        }


        private void ApplyProblemDetailsDefaults(
            HttpContext httpContext,
            ProblemDetails problemDetails,
            IDictionary<string, object?>? extensions)
        {
            problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
            {
                problemDetails.Extensions["correlationId"] = correlationId;
            }

            problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

            if (extensions is not null)
            {
                foreach (var extension in extensions)
                {
                    problemDetails.Extensions[extension.Key] = extension.Value;
                }
            }
        }
    }
}
