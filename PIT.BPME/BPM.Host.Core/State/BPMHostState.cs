using BPME.BPM.Host.Core.Interfaces;
using BPME.BPM.Host.Core.Models.Configurations;
using System.Collections.ObjectModel;

namespace BPME.BPM.Host.Core.State
{
    /// <summary>
    /// Реализация состояния движка BPM.
    ///
    /// Singleton, который хранит очередь запросов на выполнение.
    /// BPMStateListener подписывается на PendingRequests.CollectionChanged
    /// и обрабатывает новые запросы.
    /// </summary>
    public class BPMHostState : IBPMState
    {
        /// <inheritdoc />
        public ObservableCollection<StartRequest> PendingRequests { get; } = new();

        /// <inheritdoc />
        public void EnqueueRequest(StartRequest request)
        {
            PendingRequests.Add(request);
        }

        /// <inheritdoc />
        public void DequeueRequest(StartRequest request)
        {
            PendingRequests.Remove(request);
        }
    }
}
