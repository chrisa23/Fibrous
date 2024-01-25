using System;
using System.Threading.Tasks;

namespace Fibrous;

public class LockFiberFactory : IFiberFactory
{
    private readonly IAsyncFiberScheduler _asyncScheduler;
    private readonly int _size;
    private readonly TaskFactory _taskFactory;

    public LockFiberFactory(int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null,
        IAsyncFiberScheduler asyncScheduler = null)
    {
        _size = size;
        _taskFactory = taskFactory;
        _asyncScheduler = asyncScheduler;
    }

    public IAsyncFiber CreateAsyncFiber(Action<Exception> errorHandler) =>
        new LockAsyncFiber(errorHandler, _size, _taskFactory, _asyncScheduler);
}
