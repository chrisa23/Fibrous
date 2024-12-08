using System;

namespace Fibrous;

public class StubFiberFactory(IAsyncFiberScheduler asyncScheduler = null) : IFiberFactory
{
    public IFiber CreateFiber(Action<Exception> errorHandler) =>
        new StubFiber(errorHandler, asyncScheduler);
}
