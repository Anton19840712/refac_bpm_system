// =============================================================================
// TODO: SAMPLE FILE - Это тестовый файл-шаблон, будет отрефакторен
// При создании реального executor'а используйте этот файл как образец
// =============================================================================

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BPME.BPM.Host.Core.Executor.Steps
{
    /// <summary>
    /// [SAMPLE] Пример исполнителя шага.
    /// Используйте как шаблон для создания реальных executor'ов.
    ///
    /// Чтобы добавить новый тип шага:
    /// 1. Скопируйте этот файл
    /// 2. Переименуйте класс (например: HttpRequestStepExecutor)
    /// 3. Измените StepType (например: "http-request")
    /// 4. Создайте типизированный конфиг (например: HttpRequestStepConfig)
    /// 5. Реализуйте логику в ExecuteAsync
    /// 6. Зарегистрируйте в DI
    /// </summary>
    public class SampleStepExecutor : IStepExecutor
    {
        private readonly ILogger<SampleStepExecutor> _logger;

        public SampleStepExecutor(ILogger<SampleStepExecutor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Тип шага: "sample"
        /// При создании реального executor'а замените на свой тип.
        /// </summary>
        public string StepType => "sample";

        public async Task<StepExecutionResult> ExecuteAsync(
            string? settingsJson,
            Dictionary<string, object>? inputData,
            CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                // 1. Десериализуем настройки в типизированный конфиг
                var config = ParseConfig(settingsJson);

                _logger.LogDebug(
                    "[{StepType}] Выполнение с конфигом: {@Config}",
                    StepType, config);

                // 2. Выполняем логику шага
                var output = await ExecuteInternalAsync(config, inputData, cancellationToken);

                sw.Stop();

                _logger.LogInformation(
                    "[{StepType}] Успешно за {Duration:F0}ms",
                    StepType, sw.Elapsed.TotalMilliseconds);

                return StepExecutionResult.Success(output, sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();

                _logger.LogError(
                    ex,
                    "[{StepType}] Ошибка: {Error}",
                    StepType, ex.Message);

                return StepExecutionResult.Failure(ex.Message, sw.Elapsed);
            }
        }

        /// <summary>
        /// Парсинг JSON-настроек в типизированный конфиг.
        /// Замените SampleStepConfig на свой тип.
        /// </summary>
        private SampleStepConfig ParseConfig(string? settingsJson)
        {
            if (string.IsNullOrEmpty(settingsJson))
                return new SampleStepConfig();

            return JsonSerializer.Deserialize<SampleStepConfig>(settingsJson)
                   ?? new SampleStepConfig();
        }

        /// <summary>
        /// Основная логика шага.
        /// Замените на реальную реализацию.
        /// </summary>
        private async Task<object?> ExecuteInternalAsync(
            SampleStepConfig config,
            Dictionary<string, object>? inputData,
            CancellationToken cancellationToken)
        {
            // Пример: имитация работы
            await Task.Delay(config.DelayMs, cancellationToken);

            return new
            {
                message = config.Message,
                inputKeysCount = inputData?.Count ?? 0,
                timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Типизированный конфиг для sample-шага.
    /// Создайте аналогичный для каждого типа шага.
    /// </summary>
    public class SampleStepConfig
    {
        public string Message { get; set; } = "Sample step executed";
        public int DelayMs { get; set; } = 100;
    }
}
