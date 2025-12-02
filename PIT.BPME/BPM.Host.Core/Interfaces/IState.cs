namespace BPME.BPM.Host.Core.Interfaces
{
    /// <summary>
    /// Базовый интерфейс управления состоянием исполняемого элемента.
    ///
    /// Зачем нужен:
    /// - Хранит данные, которые передаются между шагами процесса
    /// - Позволяет получать и устанавливать параметры по ключу
    /// - Каждый Process и Step имеют своё состояние
    ///
    /// TState - тип модели состояния (ProcessStateModel, StepStateModel)
    /// </summary>
    /// <typeparam name="TState">Тип модели, описывающей состояние</typeparam>
    public interface IState<TState> where TState : class
    {
        /// <summary>
        /// Модель состояния, содержащая все данные
        /// </summary>
        TState State { get; }

        /// <summary>
        /// Получить параметр из состояния по ключу.
        /// Используется для чтения данных, которые установил предыдущий шаг.
        /// </summary>
        /// <typeparam name="TParameter">Тип параметра</typeparam>
        /// <param name="key">Ключ параметра</param>
        /// <returns>Значение параметра или default если не найден</returns>
        Task<TParameter?> GetParameterAsync<TParameter>(string key);

        /// <summary>
        /// Установить параметр в состояние.
        /// Используется для сохранения результата работы шага.
        /// </summary>
        /// <typeparam name="TParameter">Тип параметра</typeparam>
        /// <param name="key">Ключ параметра</param>
        /// <param name="value">Значение параметра</param>
        Task SetParameterAsync<TParameter>(string key, TParameter value);

        /// <summary>
        /// Проверить наличие параметра в состоянии
        /// </summary>
        /// <param name="key">Ключ параметра</param>
        /// <returns>true если параметр существует</returns>
        bool HasParameter(string key);

        /// <summary>
        /// Удалить параметр из состояния
        /// </summary>
        /// <param name="key">Ключ параметра</param>
        Task RemoveParameterAsync(string key);
    }
}
