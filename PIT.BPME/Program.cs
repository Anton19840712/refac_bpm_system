using BPME.BPM.Host.Core.Configuration;
using BPME.BPM.Host.Core.Data;
using BPME.BPM.Host.Core.DataBus;
using BPME.BPM.Host.Core.Executor;
using BPME.BPM.Host.Core.Executor.Steps;
using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using BPME.BPM.Host.Core.Repositories;
using BPME.BPM.Host.Core.Services;
using BPME.BPM.Host.Core.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PIT.Infrastructure.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// === Инфраструктура ===
builder.AddPitInfrastructure();

// === База данных ===

// Регистрация DbContext с PostgreSQL
// Строка подключения берётся из appsettings.json: ConnectionStrings:BpmDatabase
builder.Services.AddDbContext<BpmDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("BpmDatabase");
    options.UseNpgsql(connectionString);
});

// === Кэширование ===

// IMemoryCache — встроенный in-memory кэш ASP.NET Core
builder.Services.AddMemoryCache();

// ICacheService — наша абстракция над кэшем (Singleton)
// Позволяет в будущем заменить на Redis без изменения бизнес-логики
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

// === BPM Engine ===

// Состояние движка (Singleton) — хранит очередь запросов на выполнение
builder.Services.AddSingleton<IBPMState, BPMHostState>();

// Репозиторий конфигураций (Scoped) — работает с PostgreSQL через EF Core
builder.Services.AddScoped<IConfigureRepository<ProcessConfig>, EfProcessConfigRepository>();

// Репозиторий экземпляров процессов (Scoped) — история выполнения
builder.Services.AddScoped<IProcessInstanceRepository, EfProcessInstanceRepository>();

// Сервис конфигураций (Scoped) — бизнес-логика + валидация
builder.Services.AddScoped<IConfigureService<ProcessConfig>, ProcessConfigService>();

// Исполнитель процесса (Scoped) — создаётся для каждого запуска процесса
builder.Services.AddScoped<ProcessExecutorService>();

// === Step Executors (SAMPLE - будут отрефакторены) ===

// HttpClient для HTTP-запросов
builder.Services.AddHttpClient();

// Регистрация executor'ов — каждый обрабатывает свой StepType
builder.Services.AddScoped<IStepExecutor, SampleStepExecutor>();
builder.Services.AddScoped<IStepExecutor, HttpRequestStepExecutor>();
builder.Services.AddScoped<IStepExecutor, ScriptStepExecutor>();

// Фабрика — резолвит executor по StepType
builder.Services.AddScoped<StepExecutorFactory>();

// Слушатель состояния (HostedService) — обрабатывает запросы из очереди
builder.Services.AddHostedService<BPMStateListener>();

// === Шина данных (RabbitMQ) ===

// Конфигурация RabbitMQ из appsettings.json
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

// RabbitMQ Listener Factory — создаёт слушатели для каждой очереди из конфигурации
// Конфигурация: RabbitMQ:Queues[]
builder.Services.AddHostedService<RabbitMqListenerFactory>();

// === Тестирование (только Development) ===

// Сервис для загрузки и запуска тестовых конфигураций
builder.Services.AddScoped<TestRunnerService>();

// === Health Checks ===

// Проверка доступности БД и состояния системы
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BpmDbContext>("database", tags: new[] { "db", "ready" })
    .AddCheck("bpm-state", () =>
    {
        // Проверяем, что BPM State работает
        return HealthCheckResult.Healthy("BPM State is operational");
    }, tags: new[] { "bpm", "ready" });

WebApplication app = builder.Build();

// === Автоматическое применение миграций (только для Development) ===
// Создаёт базу данных, если её нет, и применяет все миграции
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BpmDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Применение миграций базы данных...");
    db.Database.Migrate();
    logger.LogInformation("Миграции применены успешно");
}

app.UsePitInfrastructure();

// === Health Check Endpoints ===
var healthJsonOptions = new System.Text.Json.JsonSerializerOptions
{
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    WriteIndented = false
};

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        }, healthJsonOptions);
        await context.Response.WriteAsync(result);
    }
});

// Endpoint для readiness (готовность принимать трафик)
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Endpoint для liveness (приложение живо)
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Только проверка, что приложение отвечает
});

app.Run();
