using PIT.Common.Models;

namespace PIT.Common.Interfaces
{
    public interface IControllerErrorHandler
    {
        Task<TResponseData?> TryInvokeMethodAsync<TResponseData, TRequestData>(Func<TRequestData, Task<TResponseData?>> serviceMethod, TRequestData requestData);
        Task<TResponseData?> TryInvokeMethodAsync<TResponseData, TRequestData1, TRequestData2>(Func<TRequestData1, TRequestData2, Task<TResponseData?>> serviceMethod, TRequestData1 requestData1, TRequestData2 requestData2);
        TResponseData? TryInvokeMethod<TResponseData, TRequestData>(Func<TRequestData, TResponseData?> serviceMethod, TRequestData requestData);
        Task<TResponseData?> TryInvokeMethodAsync<TResponseData>(Func<Task<TResponseData?>> serviceMethod);
        TResponseData? TryInvokeMethod<TResponseData>(Func<TResponseData?> serviceMethod);
    }
}
