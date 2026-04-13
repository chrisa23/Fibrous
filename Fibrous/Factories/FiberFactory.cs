using System;
namespace Fibrous;

public class FiberFactory(
    int size = QueueSize.DefaultQueueSize,
    IFiberScheduler scheduler = null)
    : IFiberFactory
{
    public IFiber CreateFiber(Action<Exception> errorHandler) =>
        new Fiber(errorHandler, size, scheduler);
}
