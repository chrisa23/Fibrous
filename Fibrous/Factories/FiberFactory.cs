using System;
using System.Threading.Tasks;

namespace Fibrous;

public class FiberFactory(
    int size = QueueSize.DefaultQueueSize,
    TaskFactory taskFactory = null,
    IFiberScheduler scheduler = null)
    : IFiberFactory
{
    public IFiber CreateFiber(Action<Exception> errorHandler) =>
        new Fiber(errorHandler, size, taskFactory, scheduler);
}
