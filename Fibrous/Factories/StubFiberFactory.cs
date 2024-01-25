using System;

namespace Fibrous;

public class StubFiberFactory : IFiberFactory
{
    private readonly IAsyncFiberScheduler _asyncScheduler;

    public StubFiberFactory(IAsyncFiberScheduler asyncScheduler = null)
    {
        _asyncScheduler = asyncScheduler;
    }

    public IAsyncFiber CreateAsyncFiber(Action<Exception> errorHandler) =>
        new AsyncStubFiber(errorHandler, _asyncScheduler);
}
