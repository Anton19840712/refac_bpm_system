using System.Text.Json.Serialization;

namespace PIT.Common.Models
{
    public class Page<T> where T : class
    {
        public Page() { }
        public Page(BasicFilter baseFilter)
        {
            CurrentPage = baseFilter.Page;
        }
        public IEnumerable<T?>? Items { get; set; }

        /// <summary>
        /// Общее количество объектов в базе
        /// </summary>
        public long ObjectCount { get; set; }
        /// <summary>
        /// Количество страниц
        /// </summary>
        public long PageCount { get => RequestCount == 0 ? ObjectCount == 0 ? 0 : 1 : ObjectCount / RequestCount; }
        /// <summary>
        /// Сколько было запрошено записей
        /// </summary>
        public int RequestCount { get; set; }
        /// <summary>
        /// Текущая выбранная страница
        /// </summary>
        public long CurrentPage { get; set; }
    }
}
