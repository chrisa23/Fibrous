using System;

namespace Fibrous;

public interface IFiberFactory
{
    IFiber CreateAsyncFiber(Action<Exception> errorHandler);
}
