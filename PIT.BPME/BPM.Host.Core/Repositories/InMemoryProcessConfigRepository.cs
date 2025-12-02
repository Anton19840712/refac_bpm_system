using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using System.Collections.Concurrent;

namespace BPME.BPM.Host.Core.Repositories
{
    /// <summary>
    /// In-Memory реализация репозитория конфигураций процессов.
    ///
    /// Используется для:
    /// - Разработки и отладки без БД
    /// - Unit-тестирования
    ///
    /// В продакшене заменить на EfProcessConfigRepository (Entity Framework).
    ///
    /// Lifetime: Singleton (данные сохраняются между запросами)
    /// </summary>
    public class InMemoryProcessConfigRepository : IConfigureRepository<ProcessConfig>
    {
        private readonly ConcurrentDictionary<Guid, ProcessConfig> _storage = new();

        /// <inheritdoc />
        public Task<ProcessConfig> CreateAsync(ProcessConfig config, CancellationToken cancellationToken = default)
        {
            if (config.Id == Guid.Empty)
            {
                config.Id = Guid.NewGuid();
            }

            config.CreatedAt = DateTime.UtcNow;

            if (!_storage.TryAdd(config.Id, config))
            {
                throw new InvalidOperationException($"Конфигурация с Id {config.Id} уже существует");
            }

            return Task.FromResult(config);
        }

        /// <inheritdoc />
        public Task<ProcessConfig> UpdateAsync(ProcessConfig config, CancellationToken cancellationToken = default)
        {
            config.UpdatedAt = DateTime.UtcNow;
            _storage[config.Id] = config;
            return Task.FromResult(config);
        }

        /// <inheritdoc />
        public Task<ProcessConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _storage.TryGetValue(id, out var config);
            return Task.FromResult(config);
        }

        /// <inheritdoc />
        public Task<ProcessConfig?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
        {
            var config = _storage.Values
                .FirstOrDefault(c => c.PublicId.Equals(publicId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(config);
        }

        /// <inheritdoc />
        public Task<IEnumerable<ProcessConfig>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<ProcessConfig>>(_storage.Values.ToList());
        }

        /// <inheritdoc />
        public Task<IEnumerable<ProcessConfig>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            var activeConfigs = _storage.Values.Where(c => c.IsActive).ToList();
            return Task.FromResult<IEnumerable<ProcessConfig>>(activeConfigs);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage.TryRemove(id, out _));
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string publicId, CancellationToken cancellationToken = default)
        {
            var exists = _storage.Values
                .Any(c => c.PublicId.Equals(publicId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(exists);
        }
    }
}
