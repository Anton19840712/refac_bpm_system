using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ErrorHandling.Exceptions
{
    /// <summary>
    /// Исключение для HTTP 409 Conflict.
    /// Используется когда запрос конфликтует с текущим состоянием сервера.
    /// </summary>
    public sealed class ConflictException : Exception
    {
        public ConflictException(string message)
            : base(message)
        {
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Создаёт исключение для дубликата сущности
        /// </summary>
        public static ConflictException ForDuplicate(string entityName, string fieldName, object value)
        {
            return new ConflictException($"Сущность '{entityName}' с полем '{fieldName}' = '{value}' уже существует");
        }

        /// <summary>
        /// Создаёт исключение для конфликта версий
        /// </summary>
        public static ConflictException ForVersionMismatch(string entityName, object key)
        {
            return new ConflictException($"Конфликт версий для сущности '{entityName}' с идентификатором '{key}'. Данные были изменены другим пользователем");
        }

        /// <summary>
        /// Создаёт исключение для нарушения бизнес-правила
        /// </summary>
        public static ConflictException ForBusinessRule(string rule)
        {
            return new ConflictException($"Нарушено бизнес-правило: {rule}");
        }
    }
}
