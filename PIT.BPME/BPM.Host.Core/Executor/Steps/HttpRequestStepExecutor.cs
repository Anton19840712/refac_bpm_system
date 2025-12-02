// =============================================================================
// TODO: SAMPLE FILE - Это тестовый файл-шаблон, будет отрефакторен
// Реализация HTTP-запросов — заглушка для демонстрации паттерна
// =============================================================================

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BPME.BPM.Host.Core.Executor.Steps
{
    /// <summary>
    /// [SAMPLE] Executor для HTTP-запросов.
    /// StepType: "http-request"
    /// </summary>
    public class HttpRequestStepExecutor : IStepExecutor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpRequestStepExecutor> _logger;

        public HttpRequestStepExecutor(
            IHttpClientFactory httpClientFactory,
            ILogger<HttpRequestStepExecutor> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string StepType => "http-request";

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
                    "[{StepType}] {Method} {Url}",
                    StepType, config.Method, config.Url);

                var response = await ExecuteRequestAsync(config, inputData, cancellationToken);

                sw.Stop();

                _logger.LogInformation(
                    "[{StepType}] {StatusCode} за {Duration:F0}ms",
                    StepType, response.StatusCode, sw.Elapsed.TotalMilliseconds);

                return StepExecutionResult.Success(response, sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[{StepType}] Ошибка: {Error}", StepType, ex.Message);
                return StepExecutionResult.Failure(ex.Message, sw.Elapsed);
            }
        }

        private HttpRequestStepConfig ParseConfig(string? settingsJson)
        {
            if (string.IsNullOrEmpty(settingsJson))
                throw new ArgumentException("HTTP request config is required");

            return JsonSerializer.Deserialize<HttpRequestStepConfig>(settingsJson)
                   ?? throw new ArgumentException("Invalid HTTP request config");
        }

        private async Task<HttpResponseResult> ExecuteRequestAsync(
            HttpRequestStepConfig config,
            Dictionary<string, object>? inputData,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();

            if (config.TimeoutSeconds > 0)
                client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

            var request = new HttpRequestMessage(
                new HttpMethod(config.Method),
                config.Url);

            // Добавляем заголовки
            if (config.Headers != null)
            {
                foreach (var header in config.Headers)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Добавляем тело запроса
            if (!string.IsNullOrEmpty(config.Body))
            {
                request.Content = new StringContent(
                    config.Body,
                    System.Text.Encoding.UTF8,
                    config.ContentType ?? "application/json");
            }

            var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseResult
            {
                StatusCode = (int)response.StatusCode,
                Body = body,
                IsSuccess = response.IsSuccessStatusCode
            };
        }
    }

    public class HttpRequestStepConfig
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";
        public string? Body { get; set; }
        public string? ContentType { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class HttpResponseResult
    {
        public int StatusCode { get; set; }
        public string? Body { get; set; }
        public bool IsSuccess { get; set; }
    }
}
