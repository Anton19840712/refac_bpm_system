using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.States;

namespace BPME.BPM.Host.Core.State
{
    /// <summary>
    /// Реализация состояния процесса.
    ///
    /// Это центральное хранилище данных для выполняемого процесса:
    /// - Управляет состояниями всех шагов
    /// - Хранит начальные аргументы и финальный результат
    /// - Обеспечивает передачу данных между шагами
    ///
    /// На схеме это ProcessStateService.
    ///
    /// Lifetime: Scoped (создаётся для каждого запущенного процесса)
    /// </summary>
    /// 
    // По состоянию я еще сам доделаю, когда появится маппинг, его одна из задач - работа с состоянием
    public class ProcessState : IState<ProcessStateModel>
    {
        private readonly ProcessStateModel _state;

        /// <summary>
        /// Создаёт новое состояние для процесса
        /// </summary>
        /// <param name="publicProcessId">Публичный идентификатор процесса</param>
        /// <param name="inputArguments">Начальные аргументы (с чем запустили процесс)</param>
        public ProcessState(string publicProcessId, object? inputArguments = null)
        {
            _state = new ProcessStateModel
            {
                PublicProcessId = publicProcessId,
                InputArguments = inputArguments
            };
        }

        /// <summary>
        /// Создаёт состояние из существующей модели
        /// (например, при восстановлении из БД)
        /// </summary>
        public ProcessState(ProcessStateModel existingState)
        {
            _state = existingState;
        }

        /// <inheritdoc />
        public ProcessStateModel State => _state;

        /// <inheritdoc />
        public Task<TParameter?> GetParameterAsync<TParameter>(string key)
        {
            if (_state.Parameters.TryGetValue(key, out var value) && value is TParameter typedValue)
            {
                return Task.FromResult<TParameter?>(typedValue);
            }
            return Task.FromResult<TParameter?>(default);
        }

        /// <inheritdoc />
        public Task SetParameterAsync<TParameter>(string key, TParameter value)
        {
            _state.Parameters[key] = value;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool HasParameter(string key)
        {
            return _state.Parameters.ContainsKey(key);
        }

        /// <inheritdoc />
        public Task RemoveParameterAsync(string key)
        {
            _state.Parameters.Remove(key);
            return Task.CompletedTask;
        }

        #region Управление состояниями шагов

        /// <summary>
        /// Получить или создать состояние для шага.
        /// Если состояние уже существует — возвращает его.
        /// Если нет — создаёт новое и сохраняет в коллекции.
        /// </summary>
        /// <param name="publicStepId">Идентификатор шага</param>
        /// <returns>Состояние шага</returns>
        public StepState GetOrCreateStepState(string publicStepId)
        {
            if (_state.StepStates.TryGetValue(publicStepId, out var existingStepState))
            {
                return new StepState(existingStepState);
            }

            var newStepState = new StepStateModel { PublicStepId = publicStepId };
            _state.StepStates[publicStepId] = newStepState;
            return new StepState(newStepState);
        }

        /// <summary>
        /// Получить состояние шага (если существует)
        /// </summary>
        /// <param name="publicStepId">Идентификатор шага</param>
        /// <returns>Состояние шага или null</returns>
        public StepState? GetStepState(string publicStepId)
        {
            if (_state.StepStates.TryGetValue(publicStepId, out var stepState))
            {
                return new StepState(stepState);
            }
            return null;
        }

        /// <summary>
        /// Получить выходные данные шага.
        /// Используется для передачи результата одного шага как входа для другого.
        /// </summary>
        /// <param name="publicStepId">Идентификатор шага-источника</param>
        /// <returns>OutputValue шага или null</returns>
        public object? GetStepOutput(string publicStepId)
        {
            if (_state.StepStates.TryGetValue(publicStepId, out var stepState))
            {
                return stepState.OutputValue;
            }
            return null;
        }

        /// <summary>
        /// Проверить, завершён ли шаг
        /// </summary>
        public bool IsStepCompleted(string publicStepId)
        {
            if (_state.StepStates.TryGetValue(publicStepId, out var stepState))
            {
                return stepState.Status == StepExecutionStatus.Completed;
            }
            return false;
        }

        #endregion

        #region Управление статусом процесса

        /// <summary>
        /// Отметить процесс как запущенный
        /// </summary>
        public void MarkAsRunning()
        {
            _state.Status = ProcessExecutionStatus.Running;
            _state.StartedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Отметить процесс как успешно завершённый
        /// </summary>
        public void MarkAsCompleted(object? result = null)
        {
            _state.Status = ProcessExecutionStatus.Completed;
            _state.OutputResult = result;
            _state.CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Отметить процесс как завершённый с ошибкой
        /// </summary>
        public void MarkAsFailed()
        {
            _state.Status = ProcessExecutionStatus.Failed;
            _state.CompletedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
