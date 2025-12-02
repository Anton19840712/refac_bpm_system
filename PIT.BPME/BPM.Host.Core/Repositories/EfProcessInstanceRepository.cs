using BPME.BPM.Host.Core.Data;
using BPME.BPM.Host.Core.Data.Entities;
using BPME.BPM.Host.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BPME.BPM.Host.Core.Repositories
{
    /// <summary>
    /// EF Core реализация репозитория экземпляров процессов.
    /// </summary>
    public class EfProcessInstanceRepository : IProcessInstanceRepository
    {
        private readonly BpmDbContext _context;
        private readonly ILogger<EfProcessInstanceRepository> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public EfProcessInstanceRepository(
            BpmDbContext context,
            ILogger<EfProcessInstanceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ProcessInstanceDto> CreateAsync(ProcessInstanceDto instance, CancellationToken cancellationToken = default)
        {
            var entity = ToEntity(instance);
            _context.ProcessInstances.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            instance.Id = entity.Id;
            _logger.LogDebug("Создан экземпляр процесса: {InstanceId}", instance.InstanceId);

            return instance;
        }

        /// <inheritdoc />
        public async Task<ProcessInstanceDto> UpdateAsync(ProcessInstanceDto instance, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProcessInstances
                .FirstOrDefaultAsync(e => e.InstanceId == instance.InstanceId, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException($"Экземпляр не найден: {instance.InstanceId}");
            }

            entity.Status = instance.Status;
            entity.OutputResultJson = instance.OutputResult != null
                ? JsonSerializer.Serialize(instance.OutputResult, JsonOptions)
                : null;
            entity.ErrorMessage = instance.ErrorMessage;
            entity.StartedAt = instance.StartedAt;
            entity.CompletedAt = instance.CompletedAt;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Обновлён экземпляр процесса: {InstanceId}, Status: {Status}",
                instance.InstanceId, instance.Status);

            return instance;
        }

        /// <inheritdoc />
        public async Task<ProcessInstanceDto?> GetByInstanceIdAsync(string instanceId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProcessInstances
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.InstanceId == instanceId, cancellationToken);

            return entity != null ? ToDto(entity) : null;
        }

        /// <inheritdoc />
        public async Task<ProcessInstanceDto?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProcessInstances
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync(e => e.CorrelationId == correlationId, cancellationToken);

            return entity != null ? ToDto(entity) : null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProcessInstanceDto>> GetByProcessPublicIdAsync(string processPublicId, CancellationToken cancellationToken = default)
        {
            var entities = await _context.ProcessInstances
                .AsNoTracking()
                .Where(e => e.ProcessPublicId == processPublicId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(100)
                .ToListAsync(cancellationToken);

            return entities.Select(ToDto);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProcessInstanceDto>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            var entities = await _context.ProcessInstances
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);

            return entities.Select(ToDto);
        }

        private static ProcessInstanceEntity ToEntity(ProcessInstanceDto dto)
        {
            return new ProcessInstanceEntity
            {
                Id = dto.Id,
                InstanceId = dto.InstanceId,
                CorrelationId = dto.CorrelationId,
                ProcessPublicId = dto.ProcessPublicId,
                Status = dto.Status,
                InputArgumentsJson = dto.InputArguments != null
                    ? JsonSerializer.Serialize(dto.InputArguments, JsonOptions)
                    : null,
                OutputResultJson = dto.OutputResult != null
                    ? JsonSerializer.Serialize(dto.OutputResult, JsonOptions)
                    : null,
                ErrorMessage = dto.ErrorMessage,
                Source = dto.Source,
                CreatedAt = dto.CreatedAt,
                StartedAt = dto.StartedAt,
                CompletedAt = dto.CompletedAt
            };
        }

        private static ProcessInstanceDto ToDto(ProcessInstanceEntity entity)
        {
            return new ProcessInstanceDto
            {
                Id = entity.Id,
                InstanceId = entity.InstanceId,
                CorrelationId = entity.CorrelationId,
                ProcessPublicId = entity.ProcessPublicId,
                Status = entity.Status,
                InputArguments = entity.InputArgumentsJson != null
                    ? JsonSerializer.Deserialize<object>(entity.InputArgumentsJson, JsonOptions)
                    : null,
                OutputResult = entity.OutputResultJson != null
                    ? JsonSerializer.Deserialize<object>(entity.OutputResultJson, JsonOptions)
                    : null,
                ErrorMessage = entity.ErrorMessage,
                Source = entity.Source,
                CreatedAt = entity.CreatedAt,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt
            };
        }
    }
}
