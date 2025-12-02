using BPME.BPM.Host.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BPME.BPM.Host.Core.Services
{
    /// <summary>
    /// Реализация кэша на основе IMemoryCache.
    ///
    /// Особенности:
    /// - Хранит данные в памяти процесса
    /// - Подходит для одного инстанса приложения
    /// - При перезапуске кэш очищается
    /// - Для нескольких инстансов нужен Redis
    ///
    /// Для понимания:
    /// - _keys хранит список ключей для поддержки RemoveByPrefixAsync
    /// - IMemoryCache не предоставляет способа получить все ключи
    /// - Поэтому мы отслеживаем их вручную
    ///
    /// Lifetime: Singleton
    /// </summary>
    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<InMemoryCacheService> _logger;

        // Храним список ключей для поддержки RemoveByPrefixAsync
        private readonly ConcurrentDictionary<string, byte> _keys = new();

        // Время жизни по умолчанию — 5 минут
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Создаёт сервис кэширования
        /// </summary>
        public InMemoryCacheService(
            IMemoryCache cache,
            ILogger<InMemoryCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Получить значение из кэша (async версия)
        /// </summary>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var value = _cache.Get<T>(key);

            if (value != null)
            {
                _logger.LogDebug("Cache HIT: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Cache MISS: {Key}", key);
            }

            return Task.FromResult(value);
        }

        /// <summary>
        /// Попытаться получить значение (синхронная версия для hot path)
        /// </summary>
        public bool TryGet<T>(string key, out T? value) where T : class
        {
            var found = _cache.TryGetValue(key, out value);

            if (found)
            {
                _logger.LogDebug("Cache HIT: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Cache MISS: {Key}", key);
            }

            return found;
        }

        /// <summary>
        /// Сохранить значение в кэш
        /// </summary>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            // При удалении из кэша — удаляем ключ из списка
            options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
            {
                _keys.TryRemove(evictedKey.ToString()!, out _);
                _logger.LogDebug("Cache EVICTED: {Key}", evictedKey);
            });

            _cache.Set(key, value, options);
            _keys.TryAdd(key, 0);

            _logger.LogDebug("Cache SET: {Key}, TTL: {Expiration}", key, expiration ?? DefaultExpiration);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Удалить значение из кэша
        /// </summary>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);

            _logger.LogDebug("Cache REMOVE: {Key}", key);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Удалить все значения по префиксу
        /// Например: RemoveByPrefixAsync("process:") удалит все ключи process:*
        /// </summary>
        public Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
        {
            var keysToRemove = _keys.Keys
                .Where(k => k.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keys.TryRemove(key, out _);
            }

            _logger.LogDebug("Cache REMOVE BY PREFIX: {Prefix}, removed {Count} keys", keyPrefix, keysToRemove.Count);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Получить или создать значение (Cache-Aside pattern)
        ///
        /// Это основной метод для использования:
        /// var config = await _cache.GetOrCreateAsync(
        ///     $"process:{publicId}",
        ///     async ct => await _repository.GetByPublicIdAsync(publicId, ct)
        /// );
        /// </summary>
        public async Task<T?> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default) where T : class
        {
            // Сначала проверяем кэш
            if (TryGet<T>(key, out var cached) && cached != null)
            {
                return cached;
            }

            // Не нашли — вызываем factory
            var value = await factory(cancellationToken);

            // Сохраняем в кэш только если значение не null
            if (value != null)
            {
                await SetAsync(key, value, expiration, cancellationToken);
            }

            return value;
        }
    }
}
