using System;

namespace Fibrous
{
    public interface IFiberFactory
    {
        IFiber Create();
        IFiber Create(Action<Exception> errorHandler);
        IAsyncFiber CreateAsync(Action<Exception> errorHandler);
    }
}
