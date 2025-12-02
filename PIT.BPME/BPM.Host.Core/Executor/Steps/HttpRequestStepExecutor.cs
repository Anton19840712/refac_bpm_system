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

        /// <summary>
        /// Создаёт executor для HTTP-запросов
        /// </summary>
        public HttpRequestStepExecutor(
            IHttpClientFactory httpClientFactory,
            ILogger<HttpRequestStepExecutor> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public string StepType => "http-request";

        /// <inheritdoc />
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

    /// <summary>
    /// Конфигурация для HTTP-запроса
    /// </summary>
    public class HttpRequestStepConfig
    {
        /// <summary>
        /// URL для запроса
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// HTTP метод (GET, POST, PUT, DELETE и т.д.)
        /// </summary>
        public string Method { get; set; } = "GET";

        /// <summary>
        /// Тело запроса
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Content-Type заголовок
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Дополнительные заголовки
        /// </summary>
        public Dictionary<string, string>? Headers { get; set; }

        /// <summary>
        /// Таймаут запроса в секундах
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Результат HTTP-запроса
    /// </summary>
    public class HttpResponseResult
    {
        /// <summary>
        /// HTTP статус код ответа
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Тело ответа
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Успешен ли запрос (2xx статус)
        /// </summary>
        public bool IsSuccess { get; set; }
    }
}
