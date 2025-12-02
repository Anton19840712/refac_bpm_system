using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using Microsoft.Extensions.Logging;

namespace BPME.BPM.Host.Core.Services
{
    /// <summary>
    /// Сервис управления конфигурациями процессов.
    ///
    /// Обеспечивает:
    /// - Валидацию конфигураций перед сохранением
    /// - Кэширование конфигураций (Cache-Aside pattern)
    /// - Логирование всех операций
    /// - Инвалидацию кэша при изменениях
    ///
    /// Схема кэширования:
    /// - Ключ: "process:{publicId}"
    /// - TTL: 5 минут (по умолчанию)
    /// - Инвалидация: при Update/Delete
    /// </summary>
    public class ProcessConfigService : IConfigureService<ProcessConfig>
    {
        private readonly IConfigureRepository<ProcessConfig> _repository;
        private readonly ICacheService _cache;
        private readonly ILogger<ProcessConfigService> _logger;

        // Префикс ключей кэша для конфигураций процессов
        private const string CacheKeyPrefix = "process:";

        /// <summary>
        /// Создаёт сервис конфигурации процессов
        /// </summary>
        public ProcessConfigService(
            IConfigureRepository<ProcessConfig> repository,
            ICacheService cache,
            ILogger<ProcessConfigService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ProcessConfig> CreateAsync(ProcessConfig config, CancellationToken cancellationToken = default)
        {
            // Валидация
            ValidateConfig(config);

            // Проверка уникальности PublicId
            if (await _repository.ExistsAsync(config.PublicId, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Конфигурация с PublicId '{config.PublicId}' уже существует");
            }

            var created = await _repository.CreateAsync(config, cancellationToken);

            // Сразу кладём в кэш — следующий GetByPublicIdAsync вернёт из кэша
            await _cache.SetAsync(GetCacheKey(created.PublicId), created, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Создана конфигурация процесса: {PublicId} (Id: {Id})",
                created.PublicId, created.Id);

            return created;
        }

        /// <inheritdoc />
        public async Task<ProcessConfig> UpdateAsync(ProcessConfig config, CancellationToken cancellationToken = default)
        {
            ValidateConfig(config);

            var updated = await _repository.UpdateAsync(config, cancellationToken);

            // Инвалидируем кэш и кладём обновлённую версию
            await _cache.SetAsync(GetCacheKey(updated.PublicId), updated, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Обновлена конфигурация процесса: {PublicId} (Id: {Id})",
                updated.PublicId, updated.Id);

            return updated;
        }

        /// <inheritdoc />
        public async Task<ProcessConfig?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
        {
            var cacheKey = GetCacheKey(publicId);

            // Cache-Aside: сначала кэш, потом БД
            var config = await _cache.GetOrCreateAsync(
                cacheKey,
                async ct => await _repository.GetByPublicIdAsync(publicId, ct),
                cancellationToken: cancellationToken);

            if (config == null)
            {
                _logger.LogWarning("Конфигурация процесса не найдена: {PublicId}", publicId);
            }

            return config;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProcessConfig>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            // Список активных конфигураций не кэшируем — он меняется часто
            // и обычно запрашивается редко (для UI списка)
            return await _repository.GetActiveAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Сначала получаем конфигурацию, чтобы узнать PublicId для инвалидации кэша
            var config = await _repository.GetByIdAsync(id, cancellationToken);

            var deleted = await _repository.DeleteAsync(id, cancellationToken);

            if (deleted && config != null)
            {
                // Инвалидируем кэш
                await _cache.RemoveAsync(GetCacheKey(config.PublicId), cancellationToken);

                _logger.LogInformation(
                    "Удалена конфигурация процесса: {PublicId} (Id: {Id})",
                    config.PublicId, id);
            }

            return deleted;
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string publicId, CancellationToken cancellationToken = default)
        {
            // ExistsAsync не кэшируем — используется только при создании
            return _repository.ExistsAsync(publicId, cancellationToken);
        }

        /// <summary>
        /// Формирует ключ кэша для конфигурации
        /// </summary>
        private static string GetCacheKey(string publicId) => $"{CacheKeyPrefix}{publicId}";

        /// <summary>
        /// Валидация конфигурации процесса
        /// </summary>
        private void ValidateConfig(ProcessConfig config)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(config.PublicId))
            {
                errors.Add("PublicId обязателен");
            }

            if (string.IsNullOrWhiteSpace(config.Name))
            {
                errors.Add("Name обязателен");
            }

            if (config.Steps == null || config.Steps.Count == 0)
            {
                errors.Add("Процесс должен содержать хотя бы один шаг");
            }
            else
            {
                // Проверка уникальности PublicId шагов
                var duplicateSteps = config.Steps
                    .GroupBy(s => s.PublicId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                if (duplicateSteps.Any())
                {
                    errors.Add($"Дублирующиеся PublicId шагов: {string.Join(", ", duplicateSteps)}");
                }

                // Проверка ссылок NextStepIds
                var stepIds = config.Steps.Select(s => s.PublicId).ToHashSet();
                var invalidRefs = config.Steps
                    .Where(s => s.NextStepIds != null)
                    .SelectMany(s => s.NextStepIds!)
                    .Where(id => !stepIds.Contains(id))
                    .Distinct();

                if (invalidRefs.Any())
                {
                    errors.Add($"Несуществующие шаги в NextStepIds: {string.Join(", ", invalidRefs)}");
                }
            }

            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Ошибки валидации конфигурации:\n- {string.Join("\n- ", errors)}");
            }
        }
    }
}
