using System.Diagnostics.Metrics;

namespace PIT.Infrastructure.Telemetry.Metrics
{
    /// <summary>
    /// Метрики приложения
    /// </summary>
    public sealed class ApplicationMetrics : IDisposable
    {
        private readonly Meter _meter;

        // Счётчики
        private readonly Counter<long> _requestCounter;
        private readonly Counter<long> _errorCounter;
        private readonly Counter<long> _businessOperationCounter;

        // Гистограммы
        private readonly Histogram<double> _requestDuration;
        private readonly Histogram<double> _databaseQueryDuration;
        private readonly Histogram<double> _externalApiCallDuration;

        // Gauges (наблюдаемые метрики)
        private readonly ObservableGauge<int> _activeRequests;
        private readonly ObservableGauge<long> _memoryUsage;

        private int _activeRequestsCount;

        public ApplicationMetrics(string serviceName, string serviceVersion)
        {
            _meter = new Meter(serviceName, serviceVersion);

            // Инициализация счётчиков
            _requestCounter = _meter.CreateCounter<long>(
                "app.requests.total",
                description: "Общее количество HTTP запросов");

            _errorCounter = _meter.CreateCounter<long>(
                "app.errors.total",
                description: "Общее количество ошибок");

            _businessOperationCounter = _meter.CreateCounter<long>(
                "app.business_operations.total",
                description: "Количество бизнес-операций");

            // Инициализация гистограмм
            _requestDuration = _meter.CreateHistogram<double>(
                "app.request.duration",
                unit: "ms",
                description: "Длительность обработки HTTP запросов");

            _databaseQueryDuration = _meter.CreateHistogram<double>(
                "app.database.query.duration",
                unit: "ms",
                description: "Длительность выполнения запросов к базе данных");

            _externalApiCallDuration = _meter.CreateHistogram<double>(
                "app.external_api.call.duration",
                unit: "ms",
                description: "Длительность вызовов внешних API");

            // Инициализация наблюдаемых метрик
            _activeRequests = _meter.CreateObservableGauge(
                "app.requests.active",
                () => _activeRequestsCount,
                description: "Количество активных запросов");

            _memoryUsage = _meter.CreateObservableGauge(
                "app.memory.usage",
                () => GC.GetTotalMemory(false),
                unit: "bytes",
                description: "Использование памяти приложением");
        }

        /// <summary>
        /// Увеличивает счётчик запросов
        /// </summary>
        public void IncrementRequestCount(string method, string path, int statusCode)
        {
            _requestCounter.Add(1,
                new KeyValuePair<string, object?>("http.method", method),
                new KeyValuePair<string, object?>("http.path", path),
                new KeyValuePair<string, object?>("http.status_code", statusCode));
        }

        /// <summary>
        /// Увеличивает счётчик ошибок
        /// </summary>
        public void IncrementErrorCount(string errorType, string errorMessage)
        {
            _errorCounter.Add(1,
                new KeyValuePair<string, object?>("error.type", errorType),
                new KeyValuePair<string, object?>("error.message", errorMessage));
        }

        /// <summary>
        /// Увеличивает счётчик бизнес-операций
        /// </summary>
        public void IncrementBusinessOperationCount(string operationType, string operationName, bool success)
        {
            _businessOperationCounter.Add(1,
                new KeyValuePair<string, object?>("operation.type", operationType),
                new KeyValuePair<string, object?>("operation.name", operationName),
                new KeyValuePair<string, object?>("operation.success", success));
        }

        /// <summary>
        /// Записывает длительность обработки запроса
        /// </summary>
        public void RecordRequestDuration(double durationMs, string method, string path, int statusCode)
        {
            _requestDuration.Record(durationMs,
                new KeyValuePair<string, object?>("http.method", method),
                new KeyValuePair<string, object?>("http.path", path),
                new KeyValuePair<string, object?>("http.status_code", statusCode));
        }

        /// <summary>
        /// Записывает длительность запроса к базе данных
        /// </summary>
        public void RecordDatabaseQueryDuration(double durationMs, string queryType, string tableName)
        {
            _databaseQueryDuration.Record(durationMs,
                new KeyValuePair<string, object?>("db.query.type", queryType),
                new KeyValuePair<string, object?>("db.table.name", tableName));
        }

        /// <summary>
        /// Записывает длительность вызова внешнего API
        /// </summary>
        public void RecordExternalApiCallDuration(double durationMs, string apiName, string endpoint, int statusCode)
        {
            _externalApiCallDuration.Record(durationMs,
                new KeyValuePair<string, object?>("api.name", apiName),
                new KeyValuePair<string, object?>("api.endpoint", endpoint),
                new KeyValuePair<string, object?>("api.status_code", statusCode));
        }

        /// <summary>
        /// Увеличивает счётчик активных запросов
        /// </summary>
        public void IncrementActiveRequests() => Interlocked.Increment(ref _activeRequestsCount);

        /// <summary>
        /// Уменьшает счётчик активных запросов
        /// </summary>
        public void DecrementActiveRequests() => Interlocked.Decrement(ref _activeRequestsCount);

        public void Dispose()
        {
            _meter.Dispose();
        }
    }
}
