using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ErrorHandling.Exceptions
{
    /// <summary>
    /// Исключение для HTTP 422 Unprocessable Entity.
    /// Используется когда запрос синтаксически корректен, но содержит семантические ошибки.
    /// </summary>
    public sealed class UnprocessableEntityException : Exception
    {
        public IDictionary<string, string[]>? Errors { get; }

        public UnprocessableEntityException(string message)
            : base(message)
        {
        }

        public UnprocessableEntityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UnprocessableEntityException(string message, IDictionary<string, string[]> errors)
            : base(message)
        {
            Errors = errors;
        }

        /// <summary>
        /// Создаёт исключение для бизнес-валидации с ошибками
        /// </summary>
        public static UnprocessableEntityException WithErrors(IDictionary<string, string[]> errors)
        {
            return new UnprocessableEntityException("Данные не прошли бизнес-валидацию", errors);
        }

        /// <summary>
        /// Создаёт исключение для бизнес-валидации с одной ошибкой
        /// </summary>
        public static UnprocessableEntityException WithError(string field, string error)
        {
            var errors = new Dictionary<string, string[]>
            {
                [field] = new[] { error }
            };
            return new UnprocessableEntityException("Данные не прошли бизнес-валидацию", errors);
        }
    }
}
