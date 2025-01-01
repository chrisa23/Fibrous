using System;

namespace Fibrous;

public class StubFiberFactory(IFiberScheduler scheduler = null) : IFiberFactory
{
    public IFiber CreateFiber(Action<Exception> errorHandler) =>
        new StubFiber(errorHandler, scheduler);
}
