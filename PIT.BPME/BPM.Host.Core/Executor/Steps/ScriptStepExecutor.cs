// =============================================================================
// TODO: SAMPLE FILE - Это тестовый файл-шаблон, будет отрефакторен
// Реализация скриптов — заглушка, нужен движок (Jint, Roslyn, etc.)
// =============================================================================

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BPME.BPM.Host.Core.Executor.Steps
{
    /// <summary>
    /// [SAMPLE] Executor для выполнения скриптов/выражений.
    /// StepType: "script"
    ///
    /// Примечание: это заглушка. В реальности здесь может быть:
    /// - JavaScript через Jint
    /// - C# через Roslyn
    /// - Lua
    /// - Просто JSONPath/выражения
    /// </summary>
    public class ScriptStepExecutor : IStepExecutor
    {
        private readonly ILogger<ScriptStepExecutor> _logger;

        public ScriptStepExecutor(ILogger<ScriptStepExecutor> logger)
        {
            _logger = logger;
        }

        public string StepType => "script";

        public async Task<StepExecutionResult> ExecuteAsync(
            string? settingsJson,
            Dictionary<string, object>? inputData,
            CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var config = ParseConfig(settingsJson);

                _logger.LogDebug(
                    "[{StepType}] Выполнение скрипта: {ScriptName}",
                    StepType, config.Name ?? "anonymous");

                var output = await ExecuteScriptAsync(config, inputData, cancellationToken);

                sw.Stop();

                _logger.LogInformation(
                    "[{StepType}] {ScriptName} выполнен за {Duration:F0}ms",
                    StepType, config.Name ?? "anonymous", sw.Elapsed.TotalMilliseconds);

                return StepExecutionResult.Success(output, sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[{StepType}] Ошибка: {Error}", StepType, ex.Message);
                return StepExecutionResult.Failure(ex.Message, sw.Elapsed);
            }
        }

        private ScriptStepConfig ParseConfig(string? settingsJson)
        {
            if (string.IsNullOrEmpty(settingsJson))
                throw new ArgumentException("Script config is required");

            return JsonSerializer.Deserialize<ScriptStepConfig>(settingsJson)
                   ?? throw new ArgumentException("Invalid script config");
        }

        private Task<object?> ExecuteScriptAsync(
            ScriptStepConfig config,
            Dictionary<string, object>? inputData,
            CancellationToken cancellationToken)
        {
            // TODO: Реализовать движок скриптов (Jint, Roslyn, etc.)
            // Пока просто возвращаем входные данные + метаданные

            var result = new Dictionary<string, object?>
            {
                ["scriptName"] = config.Name,
                ["scriptType"] = config.ScriptType,
                ["inputData"] = inputData,
                ["executedAt"] = DateTime.UtcNow
            };

            return Task.FromResult<object?>(result);
        }
    }

    public class ScriptStepConfig
    {
        public string? Name { get; set; }
        public string ScriptType { get; set; } = "expression"; // expression, javascript, csharp
        public string Script { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
