using System;

namespace Fibrous
{
    public interface IFiberFactory//???:IDisposable
    {
        IFiber Create(Action<Exception> errorHandler = null);
        IAsyncFiber CreateAsync(Action<Exception> errorHandler);
    }
}
