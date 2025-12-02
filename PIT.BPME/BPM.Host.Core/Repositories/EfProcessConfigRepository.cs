using BPME.BPM.Host.Core.Data;
using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BPME.BPM.Host.Core.Repositories
{
    /// <summary>
    /// Реализация репозитория конфигураций процессов на EF Core + PostgreSQL.
    ///
    /// Важно для понимания:
    /// - Репозиторий работает с Entity (БД), но возвращает DTO (API)
    /// - Преобразование через ConfigMapper
    /// - Include(Steps) — загружаем шаги вместе с процессом
    /// - AsNoTracking() — для Read-операций (не изменяем объекты)
    /// </summary>
    public class EfProcessConfigRepository : IConfigureRepository<ProcessConfig>
    {
        private readonly BpmDbContext _context;
        private readonly ILogger<EfProcessConfigRepository> _logger;

        public EfProcessConfigRepository(
            BpmDbContext context,
            ILogger<EfProcessConfigRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить конфигурацию по внутреннему Id
        /// </summary>
        public async Task<ProcessConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProcessConfigs
                .Include(p => p.Steps.OrderBy(s => s.Order))
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return entity?.ToDto();
        }

        /// <summary>
        /// Получить конфигурацию по публичному идентификатору
        /// </summary>
        public async Task<ProcessConfig?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProcessConfigs
                .Include(p => p.Steps.OrderBy(s => s.Order))
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken);

            return entity?.ToDto();
        }

        /// <summary>
        /// Получить все конфигурации
        /// </summary>
        public async Task<IEnumerable<ProcessConfig>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _context.ProcessConfigs
                .Include(p => p.Steps.OrderBy(s => s.Order))
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return entities.Select(e => e.ToDto());
        }

        /// <summary>
        /// Получить все активные конфигурации
        /// </summary>
        public async Task<IEnumerable<ProcessConfig>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _context.ProcessConfigs
                .Include(p => p.Steps.OrderBy(s => s.Order))
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return entities.Select(e => e.ToDto());
        }

        /// <summary>
        /// Создать новую конфигурацию
        /// </summary>
        public async Task<ProcessConfig> CreateAsync(ProcessConfig config, CancellationToken cancellationToken = default)
        {
            // Генерируем новый Id, если не задан
            if (config.Id == Guid.Empty)
            {
                config.Id = Guid.NewGuid();
            }

            config.CreatedAt = DateTime.UtcNow;

            var entity = config.ToEntity();

            // Устанавливаем ProcessConfigId для всех шагов
            foreach (var step in entity.Steps)
            {
                step.ProcessConfigId = entity.Id;
            }

            _context.ProcessConfigs.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Создана конфигурация процесса: {PublicId} (Id: {Id})",
                entity.PublicId, entity.Id);

            // Возвращаем DTO с актуальными данными
            return entity.ToDto();
        }

        /// <summary>
        /// Обновить существующую конфигурацию
        /// </summary>
        public async Task<ProcessConfig> UpdateAsync(ProcessConfig config, CancellationToken cancellationToken = default)
        {
            // Загружаем существующую entity БЕЗ шагов (чтобы избежать tracking issues)
            var entity = await _context.ProcessConfigs
                .FirstOrDefaultAsync(p => p.Id == config.Id, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException(
                    $"Конфигурация с Id={config.Id} не найдена");
            }

            // Обновляем основные поля
            entity.UpdateFromDto(config);

            // Удаляем старые шаги напрямую из БД (bulk delete)
            await _context.StepConfigs
                .Where(s => s.ProcessConfigId == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);

            // Добавляем новые шаги напрямую в DbSet
            var newSteps = config.Steps.Select(stepDto =>
            {
                var stepEntity = stepDto.ToEntity();
                stepEntity.ProcessConfigId = entity.Id;
                return stepEntity;
            }).ToList();

            _context.StepConfigs.AddRange(newSteps);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Обновлена конфигурация процесса: {PublicId}",
                entity.PublicId);

            // Перезагружаем для возврата полных данных
            var updated = await GetByIdAsync(entity.Id, cancellationToken);
            return updated!;
        }

        /// <summary>
        /// Удалить конфигурацию по Id
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProcessConfigs
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (entity == null)
            {
                return false;
            }

            _context.ProcessConfigs.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Удалена конфигурация процесса: {PublicId}",
                entity.PublicId);

            return true;
        }

        /// <summary>
        /// Проверить существование конфигурации по PublicId
        /// </summary>
        public async Task<bool> ExistsAsync(string publicId, CancellationToken cancellationToken = default)
        {
            return await _context.ProcessConfigs
                .AnyAsync(p => p.PublicId == publicId, cancellationToken);
        }
    }
}
