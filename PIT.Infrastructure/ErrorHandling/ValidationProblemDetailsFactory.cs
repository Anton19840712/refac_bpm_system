using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ErrorHandling
{
    /// <summary>
    /// Фабрика для преобразования FluentValidation ошибок в RFC 9457 Problem Details
    /// </summary>
    public static class ValidationProblemDetailsFactory
    {
        /// <summary>
        /// Преобразует ValidationException в ValidationProblemDetails
        /// </summary>
        public static ValidationProblemDetails CreateFromValidationException(
            ValidationException validationException,
            HttpContext httpContext,
            string baseUrl)
        {
            var errors = ConvertValidationFailuresToDictionary(validationException.Errors);

            var factory = new ProblemDetailsFactory(baseUrl);
            return factory.CreateValidationProblemDetails(
                httpContext,
                errors,
                detail: "One or more validation errors occurred. Please correct the errors and try again.");
        }

        /// <summary>
        /// Преобразует ValidationResult в ValidationProblemDetails
        /// </summary>
        public static ValidationProblemDetails CreateFromValidationResult(
            ValidationResult validationResult,
            HttpContext httpContext,
            string baseUrl)
        {
            var errors = ConvertValidationFailuresToDictionary(validationResult.Errors);

            var factory = new ProblemDetailsFactory(baseUrl);
            return factory.CreateValidationProblemDetails(
                httpContext,
                errors,
                detail: "One or more validation errors occurred. Please correct the errors and try again.");
        }

        private static Dictionary<string, string[]> ConvertValidationFailuresToDictionary(
            IEnumerable<ValidationFailure> failures)
        {
            return failures
                .GroupBy(
                    failure => ToCamelCase(failure.PropertyName),
                    failure => failure.ErrorMessage)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToArray());
        }

        private static string ToCamelCase(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;

            // Убираем префиксы типа "Request." или "Command."
            var parts = propertyName.Split('.');
            var lastPart = parts[^1];

            if (string.IsNullOrEmpty(lastPart))
                return propertyName;

            return char.ToLowerInvariant(lastPart[0]) + lastPart[1..];
        }
    }
}
