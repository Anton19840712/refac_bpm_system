
# PIT.Infrastructure - Comprehensive NuGet Package for .NET 8

Полнофункциональный инфраструктурный NuGet пакет для .NET 8 приложений с поддержкой лучших практик, чистой архитектуры и Production-ready кода.

## Содержание

- [Обзор](#%D0%BE%D0%B1%D0%B7%D0%BE%D1%80)
- [Быстрый старт](#%D0%B1%D1%8B%D1%81%D1%82%D1%80%D1%8B%D0%B9-%D1%81%D1%82%D0%B0%D1%80%D1%82)
- [Полная конфигурация](#%D0%BF%D0%BE%D0%BB%D0%BD%D0%B0%D1%8F-%D0%BA%D0%BE%D0%BD%D1%84%D0%B8%D0%B3%D1%83%D1%80%D0%B0%D1%86%D0%B8%D1%8F)
- [Error Handling](#error-handling)
- [Logging](#logging)
- [Health Checks](#health-checks)
- [Security Headers](#security-headers)
- [Correlation ID](#correlation-id)
- [OpenTelemetry](#opentelemetry)
- [Docker \& Swarm](#docker--swarm)
- [API Gateway](#api-gateway)
- [Best Practices](#best-practices)

***

## Обзор

### Что входит

- RFC 9457 Problem Details - стандартизированная обработка ошибок
- Serilog - структурированное логирование
- OpenTelemetry - метрики и трассировка
- Health Checks - проверка работоспособности
- Security Headers - защита от веб-атак
- Correlation ID - distributed tracing
- Response Compression - Gzip и Brotli
- CORS - гибкая конфигурация
- Application Info - информация о сервисе
- Sentry - мониторинг ошибок
- Docker/Swarm/Kubernetes - готовые конфигурации


### Требования

- .NET 8.0+
- PostgreSQL (опционально, для health check)
- Docker (опционально, для контейнеризации)

***

## Быстрый старт

### 1. Установка

```bash
dotnet add package PIT.Infrastructure
```


### 2. Конфигурация appsettings.json

```json
{
  "Infrastructure": {
    "EnableErrorHandling": true,
    "EnableCompression": true,
    "EnableLogging": true,
    "EnableTelemetry": true,
    "EnableCors": true,
    "EnableHealthChecks": true,
    "EnableApplicationInfo": true,
    "EnableSecurityHeaders": true,

    "Logging": {
      "ServiceName": "MyAwesomeService",
      "Environment": "Development",
      "MinimumLevel": "Information"
    }
  }
}
```


### 3. Интеграция в Program.cs

```csharp
using PIT.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Добавляем PIT.Infrastructure
builder.AddPitInfrastructure();

builder.Services.AddControllers();

var app = builder.Build();

// Используем PIT.Infrastructure
app.UsePitInfrastructure();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

Готово! Все компоненты автоматически настроены и работают.

***

## Feature Flags - управление функциональностью

Все компоненты пакета можно включать и отключать через конфигурацию. Это позволяет использовать только нужные вам фичи.

### Основные флаги

| Флаг | Тип | По умолчанию | Описание |
| :-- | :-- | :-- | :-- |
| `EnableErrorHandling` | bool | `true` | RFC 9457 Problem Details для обработки всех ошибок HTTP |
| `EnableCompression` | bool | `true` | Response Compression (Gzip и Brotli сжатие ответов) |
| `EnableLogging` | bool | `true` | Структурированное логирование с Serilog и обогащение |
| `EnableTelemetry` | bool | `true` | OpenTelemetry метрики и распределённая трассировка |
| `EnableCors` | bool | `false` | CORS политики для Cross-Origin запросов |
| `EnableHealthChecks` | bool | `true` | Health Check endpoints (startup, readiness, liveness) |
| `EnableApplicationInfo` | bool | `true` | Application Info endpoint с информацией о сервисе |
| `EnableSecurityHeaders` | bool | `true` | Security Headers (защита от XSS, Clickjacking, etc.) |
| `EnableSentry` | bool | `false` | Sentry мониторинг ошибок и профилирование |

### Error Handling - конфигурация

| Параметр | Тип | По умолчанию | Описание |
| :-- | :-- | :-- | :-- |
| `IncludeExceptionDetails` | bool | `false` | Включать детали исключений в Problem Details (только Dev!) |
| `ProblemDetailsBaseUrl` | string | `""` | Базовый URL для поля `type` в Problem Details |
| `IncludeTraceId` | bool | `true` | Добавлять Trace ID в Problem Details |
| `EnableFluentValidation` | bool | `true` | Автоматическая интеграция с FluentValidation |
| `CorrelationIdHeader` | string | `"X-Correlation-ID"` | Имя заголовка для Correlation ID |

### Compression - конфигурация

| Параметр | Тип | По умолчанию | Описание |
| :-- | :-- | :-- | :-- |
| `EnableBrotli` | bool | `true` | Включить Brotli сжатие (лучше Gzip) |
| `BrotliLevel` | string | `"Fastest"` | Уровень сжатия: Fastest, Optimal, SmallestSize |
| `EnableGzip` | bool | `true` | Включить Gzip сжатие (резервный вариант) |
| `GzipLevel` | string | `"Fastest"` | Уровень сжатия: Fastest, Optimal, SmallestSize |
| `EnableForHttps` | bool | `true` | Разрешить сжатие для HTTPS (некоторые считают небезопасным) |

### Logging - конфигурация

| Параметр | Тип | По умолчанию | Описание |
| :-- | :-- | :-- | :-- |
| `ServiceName` | string | `"Service"` | Имя сервиса для логов и трассировки |
| `Environment` | string | `"Development"` | Окружение (Development, Production, Staging) |
| `MinimumLevel` | string | `"Information"` | Минимальный уровень: Debug, Information, Warning, Error |
| `EnableConsole` | bool | `true` | Выводить логи в консоль |
| `EnableFile` | bool | `false` | Сохранять логи в файлы |
| `FilePath` | string | `"logs/service-.txt"` | Путь к файлам логов (дата добавляется автоматически) |
| `RollingInterval` | string | `"Day"` | Интервал ротации: Day, Hour, Month, Year |
| `RetainedFileCountLimit` | int | `31` | Сколько файлов хранить (остальные удаляются) |
| `OutputFormat` | string | `"Text"` | Формат вывода: Text (Dev), Json (Prod) |
| `EnrichWithEnvironment` | bool | `true` | Добавлять имя машины и окружение |
| `EnrichWithThread` | bool | `true` | Добавлять ID потока |
| `EnrichWithProcess` | bool | `true` | Добавлять ID процесса |
| `EnrichWithCorrelationId` | bool | `true` | Добавлять Correlation ID автоматически |

### Telemetry - конфигурация

| Параметр | Тип | По умолчанию | Описание |
| :-- | :-- | :-- | :-- |
| `ServiceName` | string | `"Service"` | Имя сервиса для трассировки |
| `ServiceVersion` | string | `"1.0.0"` | Версия сервиса |
| `ServiceNamespace` | string | `"Default"` | Namespace (например, компания) |
| `EnableAspNetCoreInstrumentation` | bool | `true` | Трассировка HTTP запросов ASP.NET Core |
| `EnableHttpClientInstrumentation` | bool | `true` | Трассировка исходящих HTTP запросов |
| `EnableRuntimeInstrumentation` | bool | `true` | Метрики рантайма (.NET GC, CPU, Memory) |
| `EnableConsoleExporter` | bool | `false` | Выводить метрики в консоль (для Dev) |
| `EnableOtlpExporter` | bool | `true` | Экспорт в OTLP (Jaeger, DataDog, etc.) |
| `OtlpEndpoint` | string | `"http://localhost:4317"` | Адрес OTLP коллектора |
| `OtlpProtocol` | string | `"Grpc"` | Протокол: Grpc или HttpProtobuf |
| `EnableCustomMetrics` | bool | `true` | Включить ApplicationMetrics для бизнес-метрик |

### CORS - конфигурация

| Параметр | Тип | По умолчанию | Описание |
| :-- | :-- | :-- | :-- |
| `DefaultPolicyName` | string | `"DefaultCorsPolicy"` | Имя CORS политики |
| `AllowedOrigins` | string[] | `[]` | Разрешённые домены (например: `["https://app.com"]`) |
| `AllowedMethods` | string[] | `["GET", "POST"]` | Разрешённые HTTP методы |
| `AllowedHeaders` | string[] | `["*"]` | Разрешённые заголовки (`["*"]` = все) |
| `AllowCredentials` | bool | `false` | Разрешить cookies и authentication headers |
| `ExposedHeaders` | string[] | `[]` | Какие headers видны клиенту |
| `PreflightMaxAge` | int | `3600` | Сколько секунд кэшировать preflight запросы |

### Health Checks - конфигурация

| Параметр | Тип | По умолчанию | Описание |
| :-- | :-- | :-- | :-- |
| `Endpoint` | string | `"/health"` | Общий health check endpoint |
| `ReadinessEndpoint` | string | `"/health/ready"` | Readiness probe (готовность к трафику) |
| `LivenessEndpoint` | string | `"/health/live"` | Liveness probe (приложение живо) |
| `EnableDatabaseCheck` | bool | `false` | Проверять подключение к БД |
| `DatabaseConnectionString` | string | `""` | Connection string для проверки БД |
| `DatabaseType` | string | `"PostgreSql"` | Тип БД: PostgreSql, SqlServer, MySql |
| `TimeoutSeconds` | int | `5` | Таймаут для health checks |
| `EnableDetailedOutput` | bool | `false` | Детальная информация (только Dev!) |

### Security Headers - конфигурация

| Параметр | Тип | По умолчанию | Описание |
| :-- | :-- | :-- | :-- |
| `XFrameOptions` | string | `"DENY"` | Защита от Clickjacking: DENY, SAMEORIGIN |
| `XContentTypeOptions` | string | `"nosniff"` | Защита от MIME sniffing (всегда `nosniff`) |
| `XXssProtection` | string | `"1; mode=block"` | XSS фильтр: 0, 1, 1; mode=block |
| `ReferrerPolicy` | string | `"strict-origin-when-cross-origin"` | Политика Referer заголовка |
| `PermissionsPolicy` | string | `"accelerometer=(), camera=()..."` | Доступ к API браузера |
| `ContentSecurityPolicy` | string | `"default-src 'self'..."` | CSP политика (защита от инъекций) |
| `StrictTransportSecurity` | string | `"max-age=31536000..."` | HSTS (требование HTTPS) |
| `RemoveServerHeader` | bool | `true` | Удалять заголовок Server (скрывает версию) |
| `RemoveXPoweredByHeader` | bool | `true` | Удалять X-Powered-By (скрывает технологию) |
| `EnableHstsOnlyForHttps` | bool | `true` | HSTS только для HTTPS запросов |

### Sentry - конфигурация

| Параметр | Тип | По умолчанию | Описание |
| :-- | :-- | :-- | :-- |
| `Dsn` | string | `""` | Sentry DSN (обязательный для работы) |
| `Environment` | string | `"development"` | Окружение (dev, staging, production) |
| `TracesSampleRate` | double | `0.0` | Процент трассировок (0.0 - 1.0) |
| `EnableProfiling` | bool | `false` | Включить профилирование производительности |
| `ProfilesSampleRate` | double | `0.0` | Процент профилей (0.0 - 1.0) |
| `MinimumEventLevel` | string | `"Error"` | Минимальный уровень: Debug, Info, Warning, Error |
| `EnableSerilogIntegration` | bool | `true` | Автоматическая интеграция с Serilog |

### Примеры конфигурации

**Минимальная (только ошибки и логи):**

```json
{
  "Infrastructure": {
    "EnableErrorHandling": true,
    "EnableLogging": true,
    "EnableCompression": false,
    "EnableTelemetry": false,
    "EnableHealthChecks": false,
    "EnableSecurityHeaders": false
  }
}
```

**Development (всё включено, детали):**

```json
{
  "Infrastructure": {
    "EnableErrorHandling": true,
    "EnableCompression": true,
    "EnableLogging": true,
    "EnableTelemetry": true,
    "EnableHealthChecks": true,
    "EnableSecurityHeaders": true,

    "ErrorHandling": {
      "IncludeExceptionDetails": true
    },
    "Logging": {
      "MinimumLevel": "Debug",
      "OutputFormat": "Text"
    },
    "HealthCheck": {
      "EnableDetailedOutput": true
    },
    "Telemetry": {
      "EnableConsoleExporter": true
    }
  }
}
```

**Production (безопасность, минимум деталей):**

```json
{
  "Infrastructure": {
    "EnableErrorHandling": true,
    "EnableCompression": true,
    "EnableLogging": true,
    "EnableTelemetry": true,
    "EnableHealthChecks": true,
    "EnableSecurityHeaders": true,
    "EnableSentry": true,

    "ErrorHandling": {
      "IncludeExceptionDetails": false
    },
    "Logging": {
      "MinimumLevel": "Warning",
      "OutputFormat": "Json",
      "EnableConsole": false,
      "EnableFile": true
    },
    "HealthCheck": {
      "EnableDetailedOutput": false,
      "TimeoutSeconds": 3
    },
    "Sentry": {
      "TracesSampleRate": 0.1,
      "ProfilesSampleRate": 0.05
    }
  }
}
```


***

## Error Handling

### RFC 9457 Problem Details

Автоматически обрабатывает все ошибки и возвращает стандартизированные Problem Details.

#### Поддерживаемые HTTP коды

- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 405 Method Not Allowed
- 409 Conflict
- 422 Unprocessable Entity
- 500 Internal Server Error


#### Использование в коде

```csharp
using PIT.Infrastructure.ErrorHandling.Exceptions;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetOrder(int id)
    {
        var order = _repository.GetById(id);
        if (order == null)
            throw NotFoundException.ForEntity("Order", id);

        return Ok(order);
    }

    [HttpPost]
    public IActionResult CreateOrder(CreateOrderRequest request)
    {
        if (!await _authService.HasPermissionAsync("orders.create"))
            throw ForbiddenException.ForPermission("orders.create");

        try
        {
            var order = _service.Create(request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (DuplicateOrderException ex)
        {
            throw ConflictException.ForDuplicate("Order", "OrderNumber", request.OrderNumber);
        }
    }

    [HttpPut("{id}")]
    public IActionResult UpdateOrder(int id, UpdateOrderRequest request)
    {
        var validationErrors = _validator.Validate(request);
        if (!validationErrors.IsValid)
        {
            var errors = validationErrors.GroupBy(x => x.PropertyName)
                .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray());
            throw UnprocessableEntityException.WithErrors(errors);
        }

        return Ok();
    }
}
```


#### Типизированные исключения

**ForbiddenException (403)**

```csharp
throw ForbiddenException.ForRole("Admin");
throw ForbiddenException.ForPermission("users.delete");
throw ForbiddenException.ForRoles("Admin", "Moderator");
throw new ForbiddenException("Custom message");
```

**NotFoundException (404)**

```csharp
throw NotFoundException.ForEntity("User", userId);
throw NotFoundException.ForCriteria("Product", "SKU = ABC123");
throw new NotFoundException("Custom message");
```

**ConflictException (409)**

```csharp
throw ConflictException.ForDuplicate("User", "Email", "test@example.com");
throw ConflictException.ForVersionMismatch("Document", docId);
throw ConflictException.ForBusinessRule("Cannot delete order with active items");
throw new ConflictException("Custom message");
```

**UnprocessableEntityException (422)**

```csharp
throw UnprocessableEntityException.WithError("balance", "Insufficient funds");

var errors = new Dictionary<string, string[]>
{
    ["amount"] = new[] { "Amount must be positive" },
    ["currency"] = new[] { "Unsupported currency" }
};
throw UnprocessableEntityException.WithErrors(errors);
```


#### Пример ответа (Problem Details)

```json
{
  "type": "https://api.example.com/problems/not-found",
  "title": "Not Found",
  "status": 404,
  "detail": "Сущность 'Order' с идентификатором '123' не найдена",
  "instance": "/api/orders/123",
  "traceId": "00-abc123def456-xyz789-00",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2025-11-03T14:30:00Z"
}
```


#### Пример с ошибками валидации (422)

```json
{
  "type": "https://api.example.com/problems/unprocessable-entity",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "Данные не прошли бизнес-валидацию",
  "instance": "/api/orders",
  "errors": {
    "amount": [
      "Сумма должна быть положительной"
    ],
    "currency": [
      "Неподдерживаемая валюта"
    ]
  },
  "traceId": "00-abc123def456-xyz789-00",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2025-11-03T14:30:00Z"
}
```


***

## Logging

### Структурированное логирование с Serilog

#### Development вывод (красивый, читаемый)

```
[14:30:15.234] [INF] Создание заказа OrderId=123 UserId=456
[14:30:15.456] [INF] HTTP POST /api/orders ответил 201 за 112.3456 мс
[14:30:15.678] [WRN] Попытка доступа без прав
[14:30:15.890] [ERR] Ошибка подключения к БД
System.Data.SqlClient.SqlException: Connection timeout
```


#### Production вывод (JSON структура)

```json
{
  "@t": "2025-11-03T14:30:15.234Z",
  "@mt": "Создание заказа {OrderId} пользователем {UserId}",
  "OrderId": 123,
  "UserId": 456,
  "ServiceName": "order-service",
  "Environment": "Production",
  "CorrelationId": "550e8400-e29b-41d4-a716-446655440000",
  "MachineName": "server-01",
  "ThreadId": 5,
  "ProcessId": 1234
}
```


#### Использование в коде

```csharp
_logger.LogInformation(
    "Создание заказа OrderId={OrderId} пользователем {UserId}",
    orderId,
    userId);

_logger.LogWarning(
    "Высокое время ответа БД: {Duration}ms",
    duration);

_logger.LogError(
    ex,
    "Ошибка при обработке заказа {OrderId}",
    orderId);
```


#### Автоматическое обогащение

Логи автоматически содержат:

- ServiceName - имя сервиса
- Environment - окружение (Development, Production)
- CorrelationId - ID для трейсинга
- MachineName - имя хоста
- ThreadId - ID потока
- ProcessId - ID процесса
- Timestamp - время события


#### Конфигурация файловых логов

```json
{
  "Infrastructure": {
    "Logging": {
      "EnableFile": true,
      "FilePath": "logs/service-.txt",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 31,
      "OutputFormat": "Json"
    }
  }
}
```

Логи будут сохраняться в `logs/service-20251103.txt`, `logs/service-20251104.txt` и т.д.

***

## Health Checks

### Три типа endpoints

**GET /health** - общая проверка всех компонентов
**GET /health/ready** - готовность принимать трафик (Readiness Probe)
**GET /health/live** - приложение живо (Liveness Probe)
**GET /health/startup** - приложение стартовало (Startup Probe)

### Пример ответа

```json
{
  "status": "Healthy",
  "timestamp": "2025-11-03T14:30:00Z",
  "duration": 45.23,
  "checks": [
    {
      "name": "self",
      "status": "Healthy",
      "description": "Приложение запущено и работает",
      "duration": 0.12
    },
    {
      "name": "database",
      "status": "Healthy",
      "description": "База данных доступна и работает корректно",
      "duration": 42.15,
      "data": {
        "database_type": "PostgreSql",
        "response_time_ms": 42,
        "query_result": "1"
      }
    },
    {
      "name": "memory",
      "status": "Healthy",
      "description": "Использование памяти: 128 MB",
      "duration": 2.96,
      "data": {
        "allocated_mb": 128,
        "gen0_collections": 5
      }
    }
  ]
}
```


### Тестирование

```bash
curl http://localhost:5000/health
curl http://localhost:5000/health/ready
curl http://localhost:5000/health/live
curl http://localhost:5000/health/startup
```


***

## Security Headers

### Все OWASP заголовки автоматически добавляются

**X-Frame-Options: DENY** - защита от Clickjacking
**X-Content-Type-Options: nosniff** - защита от MIME sniffing
**X-XSS-Protection: 1; mode=block** - защита от XSS
**Referrer-Policy: strict-origin-when-cross-origin** - контроль Referer
**Permissions-Policy** - контроль доступа к API браузера
**Content-Security-Policy** - защита от инъекций
**Strict-Transport-Security** - требование HTTPS

### Проверка заголовков

```bash
curl -I http://localhost:5000/api/sample

# Ожидаемый вывод:
# X-Frame-Options: DENY
# X-Content-Type-Options: nosniff
# Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
```


### Онлайн проверка

Используй https://securityheaders.com для полной проверки.

***

## Correlation ID

### Distributed Tracing через микросервисы

Correlation ID автоматически:

- Генерируется если отсутствует
- Передаётся во все логи
- Добавляется в Problem Details
- Возвращается в Response Header
- Передаётся в исходящие HTTP запросы


### Использование

```csharp
// HTTP запрос получает автоматический Correlation ID
curl -H "X-Correlation-ID: test-123" http://localhost:5000/api/orders

// Response содержит тот же ID
// X-Correlation-ID: test-123

// Все логи будут с этим ID
// "CorrelationId": "test-123"
```


### Передача между сервисами

Пакет включает `CorrelationIdHttpClientHandler` для автоматической передачи:

```csharp
builder.Services.AddHttpClient<IPaymentService, PaymentService>()
    .AddCorrelationIdHandler();
```

Теперь все запросы к Payment Service будут содержать Correlation ID.

### В Jaeger (OpenTelemetry)

Все события с одним Correlation ID объединяются в одну трассировку:

```
Trace: 550e8400-e29b-41d4-a716-446655440000

├─ Span: HTTP POST /api/orders (10ms)
├─ Span: OrderService.CreateOrder (5ms)
├─ Span: HTTP POST /api/payments (3ms)
└─ Span: Database.SaveAsync (2ms)
```


***

## OpenTelemetry

### Метрики и трассировка

Пакет автоматически экспортирует в OTLP (Jaeger, DataDog, New Relic и т.д.):

**HTTP запросы:**

- Длительность
- Статус-коды
- Размер запроса/ответа

**Вызовы БД:**

- SQL запросы
- Длительность
- Успех/ошибка

**Вызовы внешних API:**

- Адрес
- Длительность
- Статус-коды


### Использование ApplicationMetrics

```csharp
public class PaymentService
{
    private readonly ApplicationMetrics _metrics;

    public PaymentService(ApplicationMetrics metrics)
    {
        _metrics = metrics;
    }

    public async Task ProcessPaymentAsync(int orderId)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Бизнес-логика
            await _paymentGateway.ChargeAsync(orderId);

            _metrics.IncrementBusinessOperationCount(
                "payments",
                "charge_successful",
                success: true);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordDatabaseQueryDuration(duration, "INSERT", "Payments");
        }
        catch (Exception ex)
        {
            _metrics.IncrementBusinessOperationCount(
                "payments",
                "charge_failed",
                success: false);

            throw;
        }
    }
}
```


### Доступные метрики

- `IncrementRequestCount()` - HTTP запросы
- `IncrementErrorCount()` - ошибки
- `IncrementBusinessOperationCount()` - бизнес-операции
- `RecordRequestDuration()` - длительность запросов
- `RecordDatabaseQueryDuration()` - длительность БД
- `RecordExternalApiCallDuration()` - длительность внешних API


### Jaeger UI

1. Откройте http://localhost:16686
2. Выберите сервис
3. Нажмите "Find Traces"
4. Просмотрите детали трассировки

***

## Docker \& Swarm

### Dockerfile с HEALTHCHECK

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["YourApp.csproj", "."]
RUN dotnet restore "YourApp.csproj"
COPY . .
RUN dotnet build "YourApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YourApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
  CMD curl --fail http://localhost:8080/health/startup || exit 1

ENTRYPOINT ["dotnet", "YourApp.dll"]
```


### docker-compose.yml для разработки

```yaml
version: '3.8'

services:
  myapp:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/ready"]
      interval: 30s
      timeout: 3s
      retries: 3
    depends_on:
      postgres:
        condition: service_healthy

  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: user
      POSTGRES_PASSWORD: pass
      POSTGRES_DB: mydb
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U user"]
      interval: 10s
      timeout: 5s
      retries: 5
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```


### Docker Swarm deployment

```yaml
version: '3.8'

services:
  myapp:
    image: myapp:latest
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
        order: start-first
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
    expose:
      - "8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/ready"]
      interval: 30s
      timeout: 3s
      start_period: 30s
      retries: 3
    networks:
      - app-network

  postgres:
    image: postgres:16-alpine
    deploy:
      replicas: 1
      placement:
        constraints: [node.role == manager]
    environment:
      POSTGRES_USER: user
      POSTGRES_PASSWORD: pass
      POSTGRES_DB: mydb
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U user"]
      interval: 10s
      timeout: 5s
      retries: 5
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - app-network

networks:
  app-network:
    driver: overlay

volumes:
  postgres_data:
    driver: local
```


### Запуск Swarm стека

```bash
# Инициализировать Swarm
docker swarm init

# Собрать образ
docker build -t myapp:latest .

# Задеплоить стек
docker stack deploy -c docker-compose.yml myapp

# Проверить статус
docker stack services myapp
docker service ps myapp_myapp

# Обновить сервис
docker service update --image myapp:v2 myapp_myapp

# Проверить логи
docker service logs myapp_myapp
```


***

## API Gateway

### nginx интеграция

**nginx.conf:**

```nginx
upstream order_service {
    server order-service:8080;
}

upstream payment_service {
    server payment-service:8080;
}

server {
    listen 80;
    server_name api.example.com;

    # Генерировать Correlation ID если нет
    map $http_x_correlation_id $correlation_id {
        default $http_x_correlation_id;
        "" $request_id;
    }

    # Маршрут 1: /api/orders → Order Service
    location /api/orders {
        proxy_pass http://order_service;

        # Пробросить заголовки
        proxy_set_header X-Correlation-ID $correlation_id;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Host $host;

        # Логирование с Correlation ID
        access_log /var/log/nginx/access.log '$correlation_id - $status - $request_time';
    }

    # Маршрут 2: /api/payments → Payment Service
    location /api/payments {
        proxy_pass http://payment_service;

        proxy_set_header X-Correlation-ID $correlation_id;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Host $host;
    }

    # Health check для Gateway
    location /gateway/health {
        access_log off;
        return 200 "Gateway OK\n";
        add_header Content-Type text/plain;
    }
}
```


### docker-compose с Gateway

```yaml
version: '3.8'

services:
  gateway:
    image: nginx:alpine
    ports:
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
    depends_on:
      - order-service
      - payment-service

  order-service:
    image: order-service:latest
    expose:
      - "8080"
    environment:
      - ASPNETCORE_URLS=http://+:8080
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/ready"]
      interval: 30s
      timeout: 3s
      retries: 3

  payment-service:
    image: payment-service:latest
    expose:
      - "8080"
    environment:
      - ASPNETCORE_URLS=http://+:8080
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/ready"]
      interval: 30s
      timeout: 3s
      retries: 3
```


***

## Best Practices

### 1. Используйте типизированные исключения

```csharp
// Правильно
throw NotFoundException.ForEntity("User", userId);
throw ForbiddenException.ForRole("Admin");

// Неправильно
throw new Exception("User not found");
throw new UnauthorizedAccessException();
```


### 2. Структурированное логирование

```csharp
// Правильно
_logger.LogInformation("User {UserId} created order {OrderId}", userId, orderId);

// Неправильно
_logger.LogInformation($"User {userId} created order {orderId}");
```


### 3. Используйте Correlation ID

```csharp
// Автоматически добавляется во все логи
_logger.LogInformation("Processing order");

// Если нужен доступ из кода
var correlationId = HttpContext.Items["CorrelationId"];
```


### 4. Записывайте метрики

```csharp
_metrics.IncrementBusinessOperationCount("orders", "create", success: true);
_metrics.RecordDatabaseQueryDuration(duration, "SELECT", "Orders");
```


### 5. Используйте Health Checks в Docker/Kubernetes

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
  CMD curl --fail http://localhost:8080/health/ready || exit 1
```

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  periodSeconds: 10
```


### 6. Конфигурируйте по окружениям

```json
// Development: подробные ошибки, Debug логи
// Production: минимум информации, Warning+ логи
```


### 7. Мониторьте в Production

```json
{
  "Infrastructure": {
    "EnableSentry": true,
    "EnableTelemetry": true,
    "EnableFile": true
  }
}
```


***

## Troubleshooting

### Health check постоянно unhealthy

```bash
# Проверить логи
docker service logs myapp_myapp

# Проверить вручную
docker exec <container_id> curl -v http://localhost:8080/health/ready

# Увеличить start_period если медленно стартует
docker service update --health-start-period 60s myapp_myapp
```


### БД падает - все ошибки

```bash
# Временно отключить database check
# В appsettings.json: "EnableDatabaseCheck": false

# Проверить статус БД
docker service ps myapp_postgres
```


### Нет Correlation ID в логах

```bash
# Проверить что включена интеграция
"EnrichWithCorrelationId": true

# Проверить header
curl -H "X-Correlation-ID: test-123" http://localhost:5000/api/sample
```