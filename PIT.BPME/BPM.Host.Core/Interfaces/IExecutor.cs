namespace BPME.BPM.Host.Core.Interfaces
{
    /// <summary>
    /// Базовый интерфейс исполнителя.
    ///
    /// Паттерн: отделяем ДАННЫЕ (конфигурация) от ПОВЕДЕНИЯ (исполнитель).
    ///
    /// Как это работает:
    /// 1. Исполнитель получает конфигурацию (что выполнять)
    /// 2. Читает InputValue (входные данные)
    /// 3. Выполняет логику
    /// 4. Записывает результат в OutputValue
    /// 5. Всё это сохраняется в State
    ///
    /// Примеры реализаций:
    /// - ProcessExecutorService — выполняет весь процесс
    /// - StepExecutorService — выполняет один шаг
    ///
    /// Lifetime: Scoped (создаётся новый экземпляр на каждый запрос/выполнение)
    /// </summary>
    /// <typeparam name="TConfig">Тип конфигурации (ProcessConfig, StepConfig)</typeparam>
    public interface IExecutor<TConfig> where TConfig : class
    {
        /// <summary>
        /// Входные данные для исполнителя.
        /// Устанавливаются перед вызовом Execute.
        ///
        /// Пример: для HTTP-шага это может быть тело запроса
        /// </summary>
        object? InputValue { get; set; }

        /// <summary>
        /// Выходные данные после выполнения.
        /// Заполняются в процессе Execute.
        ///
        /// Пример: для HTTP-шага это ответ от сервера
        /// </summary>
        object? OutputValue { get; }

        /// <summary>
        /// Выполнить конфигурацию.
        ///
        /// Основной метод — здесь происходит вся работа.
        /// </summary>
        /// <param name="config">Конфигурация для выполнения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Конфигурация с обновлённым статусом</returns>
        Task<TConfig> ExecuteAsync(TConfig config, CancellationToken cancellationToken = default);
    }
}
