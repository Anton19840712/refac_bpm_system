using Microsoft.EntityFrameworkCore;
using PIT.Common.Extensions;
using PIT.Common.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace PIT.Common.Extensions
{
    public static class LinQExtensions
    {
        public static IQueryable<T> EntityBasicFiltration<T>(this IQueryable<T> source, BasicFilter filter)
        {
            //source = source.ExcludeOrIncludeItems(filter).NameFiltration(filter);

            return source.OrderFiltration(filter);
        }
        public static IQueryable<T> NameFiltration<T>(this IQueryable<T> source, BasicFilter filter)
        {
            if (filter.Search is not null)
            {
                var searchProp = typeof(T).GetProperty(nameof(BasicEntity.DisplayName));
                var parameter = Expression.Parameter(typeof(T).GetProperty(nameof(BasicEntity.DisplayName)).GetType(), searchProp.Name);
                var field = Expression.PropertyOrField(parameter, searchProp.Name);
                var constant = Expression.Constant(field, typeof(string));
                // Not worked now !
                source = source.Where(s => EF.Functions.ILike(constant.Value.ToString(), filter.Search));
            }

            return source;
        }
        /// <summary>
        /// Реализация постраничного получения информации
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IQueryable<T> Pagination<T>(this IQueryable<T> source, BasicFilter filter) where T : class
        {
            source = source.AsNoTracking();
            if (filter.CursorId != null
                && filter.CountOnPage.HasValue)
            {
                var count = filter.CountOnPage.GetValueOrDefault();
                source = source.Where(LambdaPagination<T>(filter));
                return source.Take(count);
            }
            else if (filter.CountOnPage.HasValue)
            {
                var skip = filter.Page * filter.CountOnPage.GetValueOrDefault();
                var count = filter.CountOnPage.GetValueOrDefault();               
                return source.Skip(skip).Take(count);
            }
            else
            {
                return source;
            }            
        }
      
        private static Expression<Func<T, bool>> LambdaPagination<T>(BasicFilter filter)
        {
            var parameter = Expression.Parameter(typeof(T), nameof(BasicEntity.CreatedAt));

            var createdAtProperty = Expression.Property(parameter, nameof(BasicEntity.CreatedAt));
            var idProperty = Expression.Property(parameter, nameof(BasicEntity.Id)); 

            var ticksConstant = Expression.Constant(filter.CreatedAtValue, typeof(long?));
            var idConstant = Expression.Constant(filter.CursorId.Value, typeof(Guid));

            var createdEarlier = Expression.LessThan(createdAtProperty, ticksConstant);

            var createdAtIsSame = Expression.Equal(createdAtProperty, ticksConstant);
            var idIsSmaller = Expression.LessThan(idProperty, idConstant);
            var tieBreaker = Expression.AndAlso(createdAtIsSame, idIsSmaller);

            var finalBody = Expression.OrElse(createdEarlier, tieBreaker);

            return Expression.Lambda<Func<T, bool>>(finalBody, parameter);
        }
        /// <summary>
        /// Определяет необходимость и порядок сортировки
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IQueryable<T> OrderFiltration<T>(this IQueryable<T> source, BasicFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.OrderBy)) source = filter.OrderByDescending ? source.OrderByDescending(filter.OrderBy) : source.OrderBy(filter.OrderBy);
            return source;
        }
        public static IQueryable<T> ExcludeOrIncludeItems<T>(this IQueryable<T> source, BasicFilter filter)
        {
            if (filter.IsDeleted == null) return source;
            var property = typeof(T).GetProperties().First(x => x.Name == nameof(BasicEntity.IsDeleted));
            return source.Where(ToLambdaExclude<T>(property, filter.IsDeleted));
        }        
        private static Expression<Func<T, bool>> ToLambdaExclude<T>(PropertyInfo prop, bool? isExclude)
        {
            if (isExclude == null) isExclude = false;
            var parameter = Expression.Parameter(typeof(T), nameof(BasicEntity.IsDeleted));
            return Expression.Lambda<Func<T, bool>>(
                Expression.Equal(
                    Expression.PropertyOrField(parameter, nameof(BasicEntity.IsDeleted)),
                    Expression.Constant(isExclude, typeof(bool?))
                    ),
                parameter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(ToLambda<T>(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyName);
            var propAsObject = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }
    }
}
