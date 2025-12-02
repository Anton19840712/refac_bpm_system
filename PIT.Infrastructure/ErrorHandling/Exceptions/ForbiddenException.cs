using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ErrorHandling.Exceptions
{
    /// <summary>
    /// Исключение для HTTP 403 Forbidden.
    /// Используется когда пользователь аутентифицирован, но не имеет прав доступа к ресурсу.
    /// </summary>
    public sealed class ForbiddenException : Exception
    {
        public ForbiddenException()
            : base("У вас нет прав для доступа к этому ресурсу")
        {
        }

        public ForbiddenException(string message)
            : base(message)
        {
        }

        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Создаёт исключение с информацией о требуемой роли
        /// </summary>
        public static ForbiddenException ForRole(string role)
        {
            return new ForbiddenException($"Требуется роль '{role}' для доступа к этому ресурсу");
        }

        /// <summary>
        /// Создаёт исключение с информацией о требуемом праве
        /// </summary>
        public static ForbiddenException ForPermission(string permission)
        {
            return new ForbiddenException($"Требуется право '{permission}' для выполнения этой операции");
        }

        /// <summary>
        /// Создаёт исключение с информацией о требуемых ролях
        /// </summary>
        public static ForbiddenException ForRoles(params string[] roles)
        {
            var rolesList = string.Join(", ", roles.Select(r => $"'{r}'"));
            return new ForbiddenException($"Требуется одна из ролей: {rolesList} для доступа к этому ресурсу");
        }
    }
}
