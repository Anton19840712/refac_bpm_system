using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace BPME.Controllers
{
    /// <summary>
    /// Контроллер управления процессами.
    ///
    /// Предоставляет API для:
    /// - Запуска процессов
    /// - Получения статуса выполнения
    /// - Отмены процессов
    ///
    /// На схеме это ProcessController.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessController : ControllerBase
    {
        private readonly IBPMState _bpmState;
        private readonly IProcessInstanceRepository _instanceRepository;
        private readonly ILogger<ProcessController> _logger;

        public ProcessController(
            IBPMState bpmState,
            IProcessInstanceRepository instanceRepository,
            ILogger<ProcessController> logger)
        {
            _bpmState = bpmState;
            _instanceRepository = instanceRepository;
            _logger = logger;
        }

        /// <summary>
        /// Запустить процесс.
        ///
        /// Добавляет запрос в очередь на выполнение.
        /// BPMStateListener подхватит его и запустит ProcessExecutorService.
        /// </summary>
        /// <param name="request">Запрос на запуск процесса</param>
        /// <returns>Информация о принятом запросе</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(StartProcessResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult StartProcess([FromBody] StartRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProcessPublicId))
            {
                return BadRequest(new { error = "ProcessPublicId обязателен" });
            }

            // Генерируем CorrelationId если не указан
            if (string.IsNullOrEmpty(request.CorrelationId))
            {
                request.CorrelationId = Guid.NewGuid().ToString();
            }

            request.Source = "API";
            request.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Получен запрос на запуск процесса {ProcessId}, CorrelationId: {CorrelationId}",
                request.ProcessPublicId,
                request.CorrelationId);

            // Добавляем в очередь
            _bpmState.EnqueueRequest(request);

            // Возвращаем 202 Accepted — запрос принят, но ещё не выполнен
            return Accepted(new StartProcessResponse
            {
                CorrelationId = request.CorrelationId,
                ProcessPublicId = request.ProcessPublicId,
                Status = "Accepted",
                Message = "Запрос на выполнение процесса принят",
                QueuedAt = request.CreatedAt
            });
        }

        /// <summary>
        /// Получить текущее количество процессов в очереди.
        /// </summary>
        [HttpGet("queue/count")]
        [ProducesResponseType(typeof(QueueStatusResponse), StatusCodes.Status200OK)]
        public IActionResult GetQueueStatus()
        {
            return Ok(new QueueStatusResponse
            {
                PendingCount = _bpmState.PendingRequests.Count,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Тестовый эндпоинт для проверки работоспособности.
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                status = "OK",
                service = "BPM Engine",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Получить статус процесса по CorrelationId.
        /// </summary>
        /// <param name="correlationId">Идентификатор корреляции</param>
        [HttpGet("status/{correlationId}")]
        [ProducesResponseType(typeof(ProcessInstanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatusByCorrelationId(string correlationId)
        {
            var instance = await _instanceRepository.GetByCorrelationIdAsync(correlationId);

            if (instance == null)
            {
                return NotFound(new { error = $"Процесс с CorrelationId '{correlationId}' не найден" });
            }

            return Ok(instance);
        }

        /// <summary>
        /// Получить статус процесса по InstanceId.
        /// </summary>
        /// <param name="instanceId">Идентификатор экземпляра</param>
        [HttpGet("instance/{instanceId}")]
        [ProducesResponseType(typeof(ProcessInstanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatusByInstanceId(string instanceId)
        {
            var instance = await _instanceRepository.GetByInstanceIdAsync(instanceId);

            if (instance == null)
            {
                return NotFound(new { error = $"Экземпляр '{instanceId}' не найден" });
            }

            return Ok(instance);
        }

        /// <summary>
        /// Получить последние N выполненных процессов.
        /// </summary>
        /// <param name="count">Количество (по умолчанию 10)</param>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(IEnumerable<ProcessInstanceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentInstances([FromQuery] int count = 10)
        {
            var instances = await _instanceRepository.GetRecentAsync(count);
            return Ok(instances);
        }

        /// <summary>
        /// Получить историю выполнения процесса по PublicId.
        /// </summary>
        /// <param name="processPublicId">PublicId конфигурации процесса</param>
        [HttpGet("history/{processPublicId}")]
        [ProducesResponseType(typeof(IEnumerable<ProcessInstanceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProcessHistory(string processPublicId)
        {
            var instances = await _instanceRepository.GetByProcessPublicIdAsync(processPublicId);
            return Ok(instances);
        }
    }

    #region DTO для ответов

    /// <summary>
    /// Ответ на запрос запуска процесса
    /// </summary>
    public class StartProcessResponse
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string ProcessPublicId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime QueuedAt { get; set; }
    }

    /// <summary>
    /// Статус очереди
    /// </summary>
    public class QueueStatusResponse
    {
        public int PendingCount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
