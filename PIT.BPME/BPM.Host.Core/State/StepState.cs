using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.States;

namespace BPME.BPM.Host.Core.State
{
    /// <summary>
    /// Реализация состояния шага.
    ///
    /// Управляет данными конкретного шага:
    /// - Хранит InputValue/OutputValue
    /// - Предоставляет доступ к параметрам
    /// - Отслеживает статус выполнения
    ///
    /// Lifetime: Scoped (создаётся для каждого выполняемого шага)
    /// </summary>
    public class StepState : IState<StepStateModel>
    {
        private readonly StepStateModel _state;

        /// <summary>
        /// Создаёт новое состояние для шага
        /// </summary>
        /// <param name="publicStepId">Идентификатор шага</param>
        public StepState(string publicStepId)
        {
            _state = new StepStateModel
            {
                PublicStepId = publicStepId
            };
        }

        /// <summary>
        /// Создаёт состояние из существующей модели
        /// (например, при восстановлении из БД)
        /// </summary>
        public StepState(StepStateModel existingState)
        {
            _state = existingState;
        }

        /// <inheritdoc />
        public StepStateModel State => _state;

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

        #region Удобные методы для работы с InputValue/OutputValue

        /// <summary>
        /// Установить входные данные шага
        /// </summary>
        public void SetInput(object? input)
        {
            _state.InputValue = input;
        }

        /// <summary>
        /// Получить входные данные шага
        /// </summary>
        public T? GetInput<T>()
        {
            return _state.InputValue is T typed ? typed : default;
        }

        /// <summary>
        /// Установить результат выполнения шага
        /// </summary>
        public void SetOutput(object? output)
        {
            _state.OutputValue = output;
        }

        /// <summary>
        /// Получить результат выполнения шага
        /// </summary>
        public T? GetOutput<T>()
        {
            return _state.OutputValue is T typed ? typed : default;
        }

        /// <summary>
        /// Отметить шаг как начатый
        /// </summary>
        public void MarkAsRunning()
        {
            _state.Status = StepExecutionStatus.Running;
            _state.StartedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Отметить шаг как успешно завершённый
        /// </summary>
        public void MarkAsCompleted()
        {
            _state.Status = StepExecutionStatus.Completed;
            _state.CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Отметить шаг как завершённый с ошибкой
        /// </summary>
        public void MarkAsFailed(string errorMessage)
        {
            _state.Status = StepExecutionStatus.Failed;
            _state.ErrorMessage = errorMessage;
            _state.CompletedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
