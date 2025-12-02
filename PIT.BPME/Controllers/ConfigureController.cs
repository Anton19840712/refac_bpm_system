using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace BPME.Controllers
{
    /// <summary>
    /// Контроллер управления конфигурациями процессов.
    ///
    /// Предоставляет CRUD API для:
    /// - Создания новых конфигураций процессов
    /// - Просмотра существующих конфигураций
    /// - Обновления конфигураций
    /// - Удаления конфигураций
    ///
    /// На схеме это ConfigureController.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigureController : ControllerBase
    {
        private readonly IConfigureService<ProcessConfig> _configService;
        private readonly ILogger<ConfigureController> _logger;

        public ConfigureController(
            IConfigureService<ProcessConfig> configService,
            ILogger<ConfigureController> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Создать новую конфигурацию процесса
        /// </summary>
        [HttpPost("process")]
        [ProducesResponseType(typeof(ProcessConfig), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProcess([FromBody] ProcessConfig config)
        {
            try
            {
                var created = await _configService.CreateAsync(config);

                _logger.LogInformation(
                    "Создана конфигурация: {PublicId}",
                    created.PublicId);

                return CreatedAtAction(
                    nameof(GetProcess),
                    new { publicId = created.PublicId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить конфигурацию процесса по PublicId
        /// </summary>
        [HttpGet("process/{publicId}")]
        [ProducesResponseType(typeof(ProcessConfig), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProcess(string publicId)
        {
            var config = await _configService.GetByPublicIdAsync(publicId);

            if (config == null)
            {
                return NotFound(new { error = $"Конфигурация '{publicId}' не найдена" });
            }

            return Ok(config);
        }

        /// <summary>
        /// Получить список всех активных конфигураций
        /// </summary>
        [HttpGet("processes")]
        [ProducesResponseType(typeof(IEnumerable<ProcessConfigSummary>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProcesses()
        {
            var configs = await _configService.GetAllActiveAsync();

            // Возвращаем краткую информацию (без шагов)
            var summaries = configs.Select(c => new ProcessConfigSummary
            {
                Id = c.Id,
                PublicId = c.PublicId,
                Name = c.Name,
                Description = c.Description,
                Version = c.Version,
                IsActive = c.IsActive,
                StepsCount = c.Steps.Count,
                CreatedAt = c.CreatedAt
            });

            return Ok(summaries);
        }

        /// <summary>
        /// Обновить конфигурацию процесса
        /// </summary>
        [HttpPut("process/{publicId}")]
        [ProducesResponseType(typeof(ProcessConfig), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProcess(string publicId, [FromBody] ProcessConfig config)
        {
            var existing = await _configService.GetByPublicIdAsync(publicId);

            if (existing == null)
            {
                return NotFound(new { error = $"Конфигурация '{publicId}' не найдена" });
            }

            // Сохраняем Id и PublicId из существующей конфигурации
            config.Id = existing.Id;
            config.PublicId = existing.PublicId;
            config.CreatedAt = existing.CreatedAt;

            try
            {
                var updated = await _configService.UpdateAsync(config);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Удалить конфигурацию процесса
        /// </summary>
        [HttpDelete("process/{publicId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProcess(string publicId)
        {
            var existing = await _configService.GetByPublicIdAsync(publicId);

            if (existing == null)
            {
                return NotFound(new { error = $"Конфигурация '{publicId}' не найдена" });
            }

            await _configService.DeleteAsync(existing.Id);

            return NoContent();
        }

        /// <summary>
        /// Проверить существование конфигурации
        /// </summary>
        [HttpHead("process/{publicId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckProcessExists(string publicId)
        {
            var exists = await _configService.ExistsAsync(publicId);
            return exists ? Ok() : NotFound();
        }
    }

    /// <summary>
    /// Краткая информация о конфигурации (для списка)
    /// </summary>
    public class ProcessConfigSummary
    {
        public Guid Id { get; set; }
        public string PublicId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public int StepsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
