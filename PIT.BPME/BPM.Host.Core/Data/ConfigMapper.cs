using BPME.BPM.Host.Core.Data.Entities;
using BPME.BPM.Host.Core.Models.Configurations;
using System.Text.Json;

namespace BPME.BPM.Host.Core.Data
{
    /// <summary>
    /// Маппер между Entity (БД) и DTO (API).
    ///
    /// Преобразует:
    /// - ProcessConfigEntity ↔ ProcessConfig
    /// - StepConfigEntity ↔ StepConfig
    /// </summary>
    public static class ConfigMapper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        #region ProcessConfig

        /// <summary>
        /// Entity → DTO
        /// </summary>
        public static ProcessConfig ToDto(this ProcessConfigEntity entity)
        {
            return new ProcessConfig
            {
                Id = entity.Id,
                PublicId = entity.PublicId,
                Name = entity.Name,
                Description = entity.Description,
                Version = entity.Version,
                IsActive = entity.IsActive,
                StartStepId = entity.StartStepId,
                Settings = DeserializeSettings(entity.SettingsJson),
                TimeoutSeconds = entity.TimeoutSeconds,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Steps = entity.Steps.Select(s => s.ToDto()).ToList()
            };
        }

        /// <summary>
        /// DTO → Entity
        /// </summary>
        public static ProcessConfigEntity ToEntity(this ProcessConfig dto)
        {
            return new ProcessConfigEntity
            {
                Id = dto.Id,
                PublicId = dto.PublicId,
                Name = dto.Name,
                Description = dto.Description,
                Version = dto.Version,
                IsActive = dto.IsActive,
                StartStepId = dto.StartStepId,
                SettingsJson = SerializeSettings(dto.Settings),
                TimeoutSeconds = dto.TimeoutSeconds,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                Steps = dto.Steps.Select(s => s.ToEntity()).ToList()
            };
        }

        /// <summary>
        /// Обновить Entity из DTO (без замены Steps)
        /// </summary>
        public static void UpdateFromDto(this ProcessConfigEntity entity, ProcessConfig dto)
        {
            entity.PublicId = dto.PublicId;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Version = dto.Version;
            entity.IsActive = dto.IsActive;
            entity.StartStepId = dto.StartStepId;
            entity.SettingsJson = SerializeSettings(dto.Settings);
            entity.TimeoutSeconds = dto.TimeoutSeconds;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        #endregion

        #region StepConfig

        /// <summary>
        /// Entity → DTO
        /// </summary>
        public static StepConfig ToDto(this StepConfigEntity entity)
        {
            return new StepConfig
            {
                PublicId = entity.PublicId,
                Name = entity.Name,
                Description = entity.Description,
                StepType = entity.StepType,
                NextStepIds = DeserializeNextStepIds(entity.NextStepIdsJson),
                Settings = DeserializeSettings(entity.SettingsJson),
                InputMapping = entity.InputMapping,
                TimeoutSeconds = entity.TimeoutSeconds,
                RetryCount = entity.RetryCount,
                Order = entity.Order
            };
        }

        /// <summary>
        /// DTO → Entity
        /// </summary>
        public static StepConfigEntity ToEntity(this StepConfig dto)
        {
            return new StepConfigEntity
            {
                Id = Guid.NewGuid(),
                PublicId = dto.PublicId,
                Name = dto.Name,
                Description = dto.Description,
                StepType = dto.StepType,
                NextStepIdsJson = SerializeNextStepIds(dto.NextStepIds),
                SettingsJson = SerializeSettings(dto.Settings),
                InputMapping = dto.InputMapping,
                TimeoutSeconds = dto.TimeoutSeconds,
                RetryCount = dto.RetryCount,
                Order = dto.Order
            };
        }

        #endregion

        #region JSON Helpers

        private static Dictionary<string, object>? DeserializeSettings(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static string? SerializeSettings(Dictionary<string, object>? settings)
        {
            if (settings == null || settings.Count == 0)
                return null;

            return JsonSerializer.Serialize(settings, JsonOptions);
        }

        private static List<string>? DeserializeNextStepIds(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static string? SerializeNextStepIds(List<string>? ids)
        {
            if (ids == null || ids.Count == 0)
                return null;

            return JsonSerializer.Serialize(ids, JsonOptions);
        }

        #endregion
    }
}
