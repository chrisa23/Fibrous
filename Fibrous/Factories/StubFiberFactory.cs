using System;

namespace Fibrous;

public class StubFiberFactory : IFiberFactory
{
    private readonly IAsyncFiberScheduler _asyncScheduler;

    public StubFiberFactory(IAsyncFiberScheduler asyncScheduler = null)
    {
        _asyncScheduler = asyncScheduler;
    }

    public IFiber CreateAsyncFiber(Action<Exception> errorHandler) =>
        new StubFiber(errorHandler, _asyncScheduler);
}
