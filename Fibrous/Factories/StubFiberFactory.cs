using System;

namespace Fibrous;

/// <summary>
///     Factory for creating stub fibers.
/// </summary>
public class StubFiberFactory(IFiberScheduler scheduler = null) : IFiberFactory
{
    public IFiber CreateFiber(Action<Exception> errorHandler) =>
        new StubFiber(errorHandler, scheduler);
}
