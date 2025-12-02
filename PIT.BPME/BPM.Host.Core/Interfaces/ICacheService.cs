namespace BPME.BPM.Host.Core.Interfaces
{
    /// <summary>
    /// Сервис кэширования.
    ///
    /// Паттерн Cache-Aside:
    /// 1. Проверяем кэш (TryGetAsync)
    /// 2. Если нет — загружаем из источника
    /// 3. Сохраняем в кэш (SetAsync)
    ///
    /// На схеме это ICacheService.
    ///
    /// Реализации:
    /// - InMemoryCacheService — для одного инстанса приложения
    /// - RedisCacheService — для распределённого кэша (несколько инстансов)
    ///
    /// Lifetime: Singleton
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Получить значение из кэша
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Значение или null, если не найдено</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Попытаться получить значение из кэша
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="value">Найденное значение</param>
        /// <returns>true если значение найдено</returns>
        bool TryGet<T>(string key, out T? value) where T : class;

        /// <summary>
        /// Сохранить значение в кэш
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="value">Значение для сохранения</param>
        /// <param name="expiration">Время жизни (по умолчанию 5 минут)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Удалить значение из кэша
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить все значения по префиксу ключа
        /// </summary>
        /// <param name="keyPrefix">Префикс ключей для удаления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить или создать значение (Cache-Aside pattern)
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="factory">Функция для создания значения, если не найдено в кэше</param>
        /// <param name="expiration">Время жизни</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Значение из кэша или созданное</returns>
        Task<T?> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default) where T : class;
    }
}
