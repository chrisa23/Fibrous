using System;

namespace Fibrous;

/// <summary>
///     Default factory for creating thread-pool-backed fibers.
/// </summary>
public class FiberFactory(
    int size = QueueSize.DefaultQueueSize,
    IFiberScheduler scheduler = null)
    : IFiberFactory
{
    public IFiber CreateFiber(Action<Exception> errorHandler) =>
        new Fiber(errorHandler, size, scheduler);
}
