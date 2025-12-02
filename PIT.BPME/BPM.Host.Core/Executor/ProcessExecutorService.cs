using BPME.BPM.Host.Core.Executor.Steps;
using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using BPME.BPM.Host.Core.State;
using Microsoft.Extensions.Logging;

namespace BPME.BPM.Host.Core.Executor
{
    /// <summary>
    /// Сервис выполнения бизнес-процесса.
    ///
    /// Это центральный компонент движка, который:
    /// - Управляет жизненным циклом процесса
    /// - Создаёт и управляет состоянием процесса
    /// - Обходит дерево шагов и выполняет их
    /// - Передаёт данные между шагами
    /// - Обрабатывает параллельное выполнение веток
    ///
    /// На схеме это ProcessExecutorService с ProcessStateService внутри.
    ///
    /// Lifetime: Scoped (создаётся для каждого запуска процесса)
    /// </summary>
    /// 
    public class ProcessExecutorService : IExecutor<ProcessConfig>
    {
        private readonly ILogger<ProcessExecutorService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly StepExecutorFactory _stepExecutorFactory;
        private ProcessState _processState = null!;
        private object? _outputValue;

        /// <summary>
        /// Создаёт сервис выполнения процесса
        /// </summary>
        public ProcessExecutorService(
            ILogger<ProcessExecutorService> logger,
            ILoggerFactory loggerFactory,
            StepExecutorFactory stepExecutorFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _stepExecutorFactory = stepExecutorFactory;
        }

        /// <inheritdoc />
        public object? InputValue { get; set; }

        /// <inheritdoc />
        public object? OutputValue => _outputValue;

        /// <summary>
        /// Состояние процесса (доступно после вызова ExecuteAsync)
        /// </summary>
        public ProcessState ProcessState => _processState;

        /// <inheritdoc />
        public async Task<ProcessConfig> ExecuteAsync(ProcessConfig config, CancellationToken cancellationToken = default)
        {
            // 1. Создаём состояние процесса
            _processState = new ProcessState(config.PublicId, InputValue);

            _logger.LogInformation(
                "Процесс {ProcessId} (Instance: {InstanceId}): начало выполнения",
                config.PublicId,
                _processState.State.ProcessInstanceId);

            _processState.MarkAsRunning();

            try
            {
                // 2. Находим стартовый шаг
                var startStep = FindStartStep(config);
                if (startStep == null)
                {
                    throw new InvalidOperationException($"Процесс {config.PublicId}: не найден стартовый шаг");
                }

                _logger.LogDebug(
                    "Процесс {ProcessId}: стартовый шаг — {StartStepId}",
                    config.PublicId,
                    startStep.PublicId);

                // 3. Выполняем дерево шагов рекурсивно
                await ExecuteStepTreeAsync(config, startStep, cancellationToken);

                // 4. Собираем финальный результат
                _outputValue = CollectFinalResult(config);
                _processState.MarkAsCompleted(_outputValue);

                _logger.LogInformation(
                    "Процесс {ProcessId} (Instance: {InstanceId}): успешно завершён",
                    config.PublicId,
                    _processState.State.ProcessInstanceId);
            }
            catch (OperationCanceledException)
            {
                _processState.MarkAsFailed();
                _logger.LogWarning(
                    "Процесс {ProcessId}: выполнение отменено",
                    config.PublicId);
                throw;
            }
            catch (Exception ex)
            {
                _processState.MarkAsFailed();
                _logger.LogError(ex,
                    "Процесс {ProcessId}: ошибка выполнения",
                    config.PublicId);
                throw;
            }

            return config;
        }

        /// <summary>
        /// Находит стартовый шаг процесса.
        ///
        /// Логика:
        /// 1. Если указан StartStepId — используем его
        /// 2. Иначе ищем шаг, на который никто не ссылается в NextStepIds
        /// </summary>
        private StepConfig? FindStartStep(ProcessConfig config)
        {
            // Если явно указан стартовый шаг
            if (!string.IsNullOrEmpty(config.StartStepId))
            {
                return config.Steps.FirstOrDefault(s => s.PublicId == config.StartStepId);
            }

            // Собираем все шаги, на которые есть ссылки
            var referencedStepIds = config.Steps
                .Where(s => s.NextStepIds != null)
                .SelectMany(s => s.NextStepIds!)
                .ToHashSet();

            // Стартовый шаг — тот, на который никто не ссылается
            return config.Steps.FirstOrDefault(s => !referencedStepIds.Contains(s.PublicId));
        }

        /// <summary>
        /// Рекурсивно выполняет дерево шагов.
        ///
        /// Алгоритм:
        /// 1. Получаем входные данные для текущего шага
        /// 2. Создаём StepExecutorService и выполняем шаг
        /// 3. Если есть следующие шаги — выполняем их параллельно
        /// </summary>
        private async Task ExecuteStepTreeAsync(
            ProcessConfig config,
            StepConfig currentStep,
            CancellationToken cancellationToken)
        {
            // 1. Получаем или создаём состояние для шага
            var stepState = _processState.GetOrCreateStepState(currentStep.PublicId);

            // 2. Определяем входные данные для шага
            var stepInput = ResolveStepInput(currentStep);

            // 3. Создаём исполнитель шага
            var stepExecutor = new StepExecutorService(
                stepState,
                _stepExecutorFactory,
                _loggerFactory.CreateLogger<StepExecutorService>())
            {
                InputValue = stepInput
            };

            // 4. Выполняем шаг
            await stepExecutor.ExecuteAsync(currentStep, cancellationToken);

            // 5. Если есть следующие шаги — выполняем их
            if (currentStep.NextStepIds != null && currentStep.NextStepIds.Count > 0)
            {
                await ExecuteNextStepsAsync(config, currentStep.NextStepIds, cancellationToken);
            }
        }

        /// <summary>
        /// Определяет входные данные для шага.
        ///
        /// Приоритет:
        /// 1. Если есть InputMapping — используем его (TODO: реализовать парсинг)
        /// 2. Иначе — берём InputArguments процесса (для первого шага)
        ///    или OutputValue предыдущего шага
        /// </summary>
        private object? ResolveStepInput(StepConfig stepConfig)
        {
            // TODO: Реализовать парсинг InputMapping (JSONPath или подобное)
            // Пока просто возвращаем InputArguments процесса
            return _processState.State.InputArguments;
        }

        /// <summary>
        /// Выполняет следующие шаги (возможно параллельно).
        /// </summary>
        private async Task ExecuteNextStepsAsync(
            ProcessConfig config,
            List<string> nextStepIds,
            CancellationToken cancellationToken)
        {
            if (nextStepIds.Count == 1)
            {
                // Один следующий шаг — выполняем последовательно
                var nextStep = config.Steps.First(s => s.PublicId == nextStepIds[0]);
                await ExecuteStepTreeAsync(config, nextStep, cancellationToken);
            }
            else
            {
                // Несколько следующих шагов — выполняем параллельно
                _logger.LogDebug(
                    "Параллельное выполнение {Count} веток: {StepIds}",
                    nextStepIds.Count,
                    string.Join(", ", nextStepIds));

                await Parallel.ForEachAsync(
                    nextStepIds,
                    cancellationToken,
                    async (nextStepId, ct) =>
                    {
                        var nextStep = config.Steps.First(s => s.PublicId == nextStepId);
                        await ExecuteStepTreeAsync(config, nextStep, ct);
                    });
            }
        }

        /// <summary>
        /// Собирает финальный результат процесса.
        ///
        /// Берёт OutputValue из всех конечных шагов (у которых нет NextStepIds).
        /// </summary>
        private object? CollectFinalResult(ProcessConfig config)
        {
            // Находим конечные шаги (без следующих)
            var finalSteps = config.Steps
                .Where(s => s.NextStepIds == null || s.NextStepIds.Count == 0)
                .ToList();

            if (finalSteps.Count == 1)
            {
                // Один конечный шаг — его OutputValue и есть результат
                return _processState.GetStepOutput(finalSteps[0].PublicId);
            }

            // Несколько конечных шагов — собираем в словарь
            var results = new Dictionary<string, object?>();
            foreach (var step in finalSteps)
            {
                results[step.PublicId] = _processState.GetStepOutput(step.PublicId);
            }

            return results;
        }
    }
}
