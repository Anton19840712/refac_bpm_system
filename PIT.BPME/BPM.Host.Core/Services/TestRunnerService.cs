using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BPME.BPM.Host.Core.Services
{
    /// <summary>
    /// Сервис для тестирования BPM Engine.
    ///
    /// Позволяет:
    /// - Загружать тестовые конфигурации из JSON-файлов
    /// - Регистрировать их в системе
    /// - Запускать на выполнение
    /// - Получать результаты
    ///
    /// Только для Development!
    /// </summary>
    public class TestRunnerService
    {
        private readonly IConfigureService<ProcessConfig> _configService;
        private readonly IBPMState _bpmState;
        private readonly ILogger<TestRunnerService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Создаёт сервис тестирования
        /// </summary>
        public TestRunnerService(
            IConfigureService<ProcessConfig> configService,
            IBPMState bpmState,
            ILogger<TestRunnerService> logger)
        {
            _configService = configService;
            _bpmState = bpmState;
            _logger = logger;
        }

        /// <summary>
        /// Получить список доступных тестовых конфигураций
        /// </summary>
        public async Task<IEnumerable<TestConfigInfo>> GetAvailableConfigsAsync()
        {
            var testDataPath = GetTestDataPath();
            var configsPath = Path.Combine(testDataPath, "Configs");

            if (!Directory.Exists(configsPath))
            {
                return Enumerable.Empty<TestConfigInfo>();
            }

            var configs = new List<TestConfigInfo>();
            var files = Directory.GetFiles(configsPath, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var config = JsonSerializer.Deserialize<ProcessConfig>(json, JsonOptions);

                    if (config != null)
                    {
                        configs.Add(new TestConfigInfo
                        {
                            FileName = Path.GetFileName(file),
                            PublicId = config.PublicId,
                            Name = config.Name,
                            Description = config.Description,
                            StepsCount = config.Steps?.Count ?? 0
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось прочитать файл: {File}", file);
                }
            }

            return configs;
        }

        /// <summary>
        /// Загрузить конфигурацию из файла и зарегистрировать в системе
        /// </summary>
        public async Task<ProcessConfig> LoadAndRegisterAsync(string fileName)
        {
            var config = await LoadConfigFromFileAsync(fileName);

            // Проверяем, существует ли уже
            var existing = await _configService.GetByPublicIdAsync(config.PublicId);

            if (existing != null)
            {
                _logger.LogInformation(
                    "Конфигурация {PublicId} уже существует, обновляем",
                    config.PublicId);

                config.Id = existing.Id;
                config.CreatedAt = existing.CreatedAt;
                return await _configService.UpdateAsync(config);
            }

            return await _configService.CreateAsync(config);
        }

        /// <summary>
        /// Загрузить конфигурацию и сразу запустить процесс
        /// </summary>
        public async Task<TestRunResult> LoadAndRunAsync(string fileName, object? inputArguments = null)
        {
            var startTime = DateTime.UtcNow;
            var correlationId = Guid.NewGuid().ToString();

            try
            {
                // 1. Загружаем и регистрируем конфигурацию
                var config = await LoadAndRegisterAsync(fileName);

                // 2. Создаём запрос на запуск
                var request = new StartRequest
                {
                    ProcessPublicId = config.PublicId,
                    CorrelationId = correlationId,
                    InputArguments = inputArguments,
                    Source = "TestRunner"
                };

                // 3. Добавляем в очередь
                _bpmState.EnqueueRequest(request);

                _logger.LogInformation(
                    "Тест запущен: {FileName} → {PublicId}, CorrelationId: {CorrelationId}",
                    fileName, config.PublicId, correlationId);

                return new TestRunResult
                {
                    Success = true,
                    CorrelationId = correlationId,
                    ProcessPublicId = config.PublicId,
                    Message = "Процесс добавлен в очередь на выполнение",
                    StartedAt = startTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка запуска теста: {FileName}", fileName);

                return new TestRunResult
                {
                    Success = false,
                    CorrelationId = correlationId,
                    Message = ex.Message,
                    StartedAt = startTime
                };
            }
        }

        /// <summary>
        /// Загрузить все тестовые конфигурации и зарегистрировать в системе
        /// </summary>
        public async Task<int> LoadAllConfigsAsync()
        {
            var configs = await GetAvailableConfigsAsync();
            var count = 0;

            foreach (var configInfo in configs)
            {
                try
                {
                    await LoadAndRegisterAsync(configInfo.FileName);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось загрузить: {FileName}", configInfo.FileName);
                }
            }

            _logger.LogInformation("Загружено {Count} тестовых конфигураций", count);
            return count;
        }

        /// <summary>
        /// Загрузить конфигурацию из файла (без регистрации)
        /// </summary>
        private async Task<ProcessConfig> LoadConfigFromFileAsync(string fileName)
        {
            var testDataPath = GetTestDataPath();
            var filePath = Path.Combine(testDataPath, "Configs", fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Тестовый файл не найден: {fileName}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var config = JsonSerializer.Deserialize<ProcessConfig>(json, JsonOptions);

            if (config == null)
            {
                throw new InvalidOperationException($"Не удалось десериализовать: {fileName}");
            }

            return config;
        }

        /// <summary>
        /// Получить путь к папке TestData
        /// </summary>
        private static string GetTestDataPath()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            // В development — ищем в корне проекта
            var projectRoot = Directory.GetParent(basePath)?.Parent?.Parent?.Parent?.FullName;

            if (projectRoot != null)
            {
                var testDataPath = Path.Combine(projectRoot, "TestData");
                if (Directory.Exists(testDataPath))
                {
                    return testDataPath;
                }
            }

            // Fallback — рядом с exe
            return Path.Combine(basePath, "TestData");
        }
    }

    /// <summary>
    /// Информация о тестовой конфигурации
    /// </summary>
    public class TestConfigInfo
    {
        /// <summary>
        /// Имя файла конфигурации
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Публичный идентификатор процесса
        /// </summary>
        public string PublicId { get; set; } = string.Empty;

        /// <summary>
        /// Название процесса
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание процесса
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Количество шагов
        /// </summary>
        public int StepsCount { get; set; }
    }

    /// <summary>
    /// Результат запуска теста
    /// </summary>
    public class TestRunResult
    {
        /// <summary>
        /// Успешен ли запуск
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Идентификатор корреляции для отслеживания
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// PublicId запущенного процесса
        /// </summary>
        public string? ProcessPublicId { get; set; }

        /// <summary>
        /// Сообщение о результате
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Время запуска
        /// </summary>
        public DateTime StartedAt { get; set; }
    }
}
