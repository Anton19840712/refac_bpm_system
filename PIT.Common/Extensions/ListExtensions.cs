using PIT.Common.Models;

namespace PIT.Common.Extensions
{
    public static class ListExtensions
    {
        public static List<T> EntityBasicFiltration<T>(this List<T> source, BasicFilter filter) where T : class
        {
            return source.Pagination(filter);
        }

        /// <summary>
        /// Реализация постраничного получения информации
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<T> Pagination<T>(this List<T> source, BasicFilter filter) where T : class
        {
            if (filter.CountOnPage.HasValue)
            {
                var skip = filter.Page * filter.CountOnPage.GetValueOrDefault();
                var count = filter.CountOnPage.GetValueOrDefault();
                return (List<T>)source.Skip(skip).Take(count);
            }
            else
            {
                return source;
            }
        }
    }
}
