using BPME.BPM.Host.Core.Executor.Steps;
using BPME.BPM.Host.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace BPME.Controllers
{
    /// <summary>
    /// Контроллер для тестирования BPM Engine.
    ///
    /// Доступен ТОЛЬКО в Development!
    ///
    /// Позволяет:
    /// - Просмотреть доступные тестовые конфигурации
    /// - Загрузить их в систему
    /// - Запустить на выполнение
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly TestRunnerService _testRunner;
        private readonly StepExecutorFactory _stepExecutorFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<TestController> _logger;

        public TestController(
            TestRunnerService testRunner,
            StepExecutorFactory stepExecutorFactory,
            IWebHostEnvironment environment,
            ILogger<TestController> logger)
        {
            _testRunner = testRunner;
            _stepExecutorFactory = stepExecutorFactory;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Проверка, что мы в Development
        /// </summary>
        private IActionResult? CheckDevelopmentOnly()
        {
            if (!_environment.IsDevelopment())
            {
                return StatusCode(403, new { error = "Доступно только в Development" });
            }
            return null;
        }

        /// <summary>
        /// Получить список доступных тестовых конфигураций
        /// </summary>
        [HttpGet("configs")]
        [ProducesResponseType(typeof(IEnumerable<TestConfigInfo>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableConfigs()
        {
            var check = CheckDevelopmentOnly();
            if (check != null) return check;

            var configs = await _testRunner.GetAvailableConfigsAsync();
            return Ok(configs);
        }

        /// <summary>
        /// Загрузить все тестовые конфигурации в систему
        /// </summary>
        [HttpPost("load-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LoadAllConfigs()
        {
            var check = CheckDevelopmentOnly();
            if (check != null) return check;

            var count = await _testRunner.LoadAllConfigsAsync();

            return Ok(new
            {
                message = $"Загружено {count} конфигураций",
                count
            });
        }

        /// <summary>
        /// Загрузить конкретную конфигурацию
        /// </summary>
        [HttpPost("load/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LoadConfig(string fileName)
        {
            var check = CheckDevelopmentOnly();
            if (check != null) return check;

            try
            {
                var config = await _testRunner.LoadAndRegisterAsync(fileName);

                return Ok(new
                {
                    message = "Конфигурация загружена",
                    publicId = config.PublicId,
                    name = config.Name,
                    stepsCount = config.Steps.Count
                });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { error = $"Файл не найден: {fileName}" });
            }
        }

        /// <summary>
        /// Запустить тест (загрузить конфигурацию + запустить процесс)
        /// </summary>
        [HttpPost("run/{fileName}")]
        [ProducesResponseType(typeof(TestRunResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> RunTest(string fileName, [FromBody] RunTestRequest? request = null)
        {
            var check = CheckDevelopmentOnly();
            if (check != null) return check;

            var result = await _testRunner.LoadAndRunAsync(fileName, request?.InputArguments);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Запустить все тесты
        /// </summary>
        [HttpPost("run-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RunAllTests()
        {
            var check = CheckDevelopmentOnly();
            if (check != null) return check;

            var configs = await _testRunner.GetAvailableConfigsAsync();
            var results = new List<TestRunResult>();

            foreach (var config in configs)
            {
                var result = await _testRunner.LoadAndRunAsync(config.FileName);
                results.Add(result);
            }

            return Ok(new
            {
                message = $"Запущено {results.Count} тестов",
                results
            });
        }

        // =====================================================================
        // Step Executors Testing
        // =====================================================================

        /// <summary>
        /// Получить список зарегистрированных Step Executors
        /// </summary>
        [HttpGet("executors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetExecutors()
        {
            var check = CheckDevelopmentOnly();
            if (check != null) return check;

            return Ok(new
            {
                supportedTypes = _stepExecutorFactory.SupportedStepTypes,
                count = _stepExecutorFactory.SupportedStepTypes.Count
            });
        }

        /// <summary>
        /// Выполнить конкретный Step Executor
        /// </summary>
        [HttpPost("executors/{stepType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExecuteStep(
            string stepType,
            [FromBody] ExecuteStepRequest? request = null)
        {
            var check = CheckDevelopmentOnly();
            if (check != null) return check;

            var executor = _stepExecutorFactory.GetExecutor(stepType);
            if (executor == null)
            {
                return NotFound(new
                {
                    error = $"Executor для типа '{stepType}' не найден",
                    availableTypes = _stepExecutorFactory.SupportedStepTypes
                });
            }

            var result = await executor.ExecuteAsync(
                request?.SettingsJson,
                request?.InputData);

            return Ok(new
            {
                stepType,
                result.IsSuccess,
                result.Output,
                result.ErrorMessage,
                durationMs = result.Duration.TotalMilliseconds
            });
        }

        /// <summary>
        /// Выполнить все Step Executors для проверки
        /// </summary>
        [HttpPost("executors/run-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExecuteAllSteps()
        {
            var check = CheckDevelopmentOnly();
            if (check != null) return check;

            var results = new List<object>();
            var testInput = new Dictionary<string, object>
            {
                ["testKey"] = "testValue",
                ["number"] = 42
            };

            foreach (var stepType in _stepExecutorFactory.SupportedStepTypes)
            {
                var executor = _stepExecutorFactory.GetExecutor(stepType)!;
                var result = await executor.ExecuteAsync(null, testInput);

                results.Add(new
                {
                    stepType,
                    result.IsSuccess,
                    result.Output,
                    result.ErrorMessage,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }

            return Ok(new
            {
                message = $"Выполнено {results.Count} executor'ов",
                results
            });
        }
    }

    /// <summary>
    /// Запрос на выполнение Step Executor
    /// </summary>
    public class ExecuteStepRequest
    {
        /// <summary>
        /// JSON с настройками шага
        /// </summary>
        public string? SettingsJson { get; set; }

        /// <summary>
        /// Входные данные процесса
        /// </summary>
        public Dictionary<string, object>? InputData { get; set; }
    }

    /// <summary>
    /// Запрос на запуск теста
    /// </summary>
    public class RunTestRequest
    {
        /// <summary>
        /// Входные аргументы для процесса
        /// </summary>
        public object? InputArguments { get; set; }
    }
}
