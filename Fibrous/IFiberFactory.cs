using System;

namespace Fibrous;

public interface IFiberFactory
{
    IAsyncFiber CreateAsyncFiber(Action<Exception> errorHandler);
}
