using System.Collections.ObjectModel;
using BPME.BPM.Host.Core.Models.Configurations;

namespace BPME.BPM.Host.Core.Interfaces
{
    /// <summary>
    /// Состояние движка BPM.
    ///
    /// Это Singleton, который:
    /// - Хранит очередь запросов на выполнение процессов
    /// - Уведомляет подписчиков о новых запросах (через ObservableCollection)
    ///
    /// На схеме это IBPMState с ObservableCollection&lt;StartRequest&gt;.
    ///
    /// Подписчик: BPMStateListener (HostedService)
    /// </summary>
    public interface IBPMState
    {
        /// <summary>
        /// Очередь запросов на выполнение процессов.
        ///
        /// BPMStateListener подписывается на CollectionChanged
        /// и обрабатывает новые запросы.
        /// </summary>
        ObservableCollection<StartRequest> PendingRequests { get; }

        /// <summary>
        /// Добавить запрос на выполнение процесса.
        /// Вызывается из контроллера или шины данных.
        /// </summary>
        /// <param name="request">Запрос на запуск процесса</param>
        void EnqueueRequest(StartRequest request);

        /// <summary>
        /// Удалить запрос из очереди (после начала обработки).
        /// </summary>
        /// <param name="request">Запрос</param>
        void DequeueRequest(StartRequest request);
    }
}
