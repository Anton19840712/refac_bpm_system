
namespace PIT.Common.Models
{
    /// <summary>
    /// Базовый фильтр, используемый в системе
    /// </summary>
    public class BasicFilter
    {
        public Guid? CursorId { get; set; }
        /// <summary>
        /// Указатель, откуда начать пагинацию
        /// </summary>
        public long? CreatedAtValue { get; set; }
        /// <summary>
        /// Номер запрашиваемой страницы с объектами 
        /// </summary>
        public int Page { get; set; } = 0;
        /// <summary>
        /// Количество возвращаемых записей на странице (null: вернуть все записи на одной странице)
        /// </summary>
        public int? CountOnPage { get; set; }

        /// <summary>
        /// Строка поиска поля по которым осуществляется поиск зависят от запрашиваемых данных
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Поле модели ответа по которому произвести сортировку
        /// </summary>
        public string? OrderBy { get; set; }

        /// <summary>
        /// Порядок сортировки
        /// </summary>
        public bool OrderByDescending { get; set; } = false;

        /// <summary>
        /// Проверка задано ли значение для поиска и его форматирование для запроса
        /// </summary>
        /// <returns></returns>
        /// Search = string.Format("%{0}%", Search);
        public bool GetSearchString()
        {

            Search = Search?.Trim();
            if (!string.IsNullOrEmpty(Search))
            {
                Search = Search;
                return true;
            }
            else return false;
        }

        public bool? IsDeleted { get; set; }
    }
}
