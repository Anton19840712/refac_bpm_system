using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ErrorHandling.Exceptions
{
    /// <summary>
    /// Исключение для HTTP 404 Not Found.
    /// Используется когда запрашиваемый ресурс не найден.
    /// </summary>
    public sealed class NotFoundException : Exception
    {
        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Создаёт исключение для сущности с указанным идентификатором
        /// </summary>
        public static NotFoundException ForEntity(string entityName, object key)
        {
            return new NotFoundException($"Сущность '{entityName}' с идентификатором '{key}' не найдена");
        }

        /// <summary>
        /// Создаёт исключение для сущности с указанными критериями
        /// </summary>
        public static NotFoundException ForCriteria(string entityName, string criteria)
        {
            return new NotFoundException($"Сущность '{entityName}' с критериями '{criteria}' не найдена");
        }
    }
}
