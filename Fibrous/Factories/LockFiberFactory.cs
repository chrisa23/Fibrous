using System;
using System.Threading.Tasks;

namespace Fibrous;

public class LockFiberFactory(
    int size = QueueSize.DefaultQueueSize,
    TaskFactory taskFactory = null,
    IAsyncFiberScheduler asyncScheduler = null)
    : IFiberFactory
{
    public IFiber CreateFiber(Action<Exception> errorHandler) =>
        new LockFiber(errorHandler, size, taskFactory, asyncScheduler);
}
