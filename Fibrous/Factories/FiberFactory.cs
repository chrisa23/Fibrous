using System;
using System.Threading.Tasks;

namespace Fibrous;

public class FiberFactory : IFiberFactory
{
    private readonly IAsyncFiberScheduler _asyncScheduler;
    private readonly int _size;
    private readonly TaskFactory _taskFactory;

    public FiberFactory(int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null,
        IAsyncFiberScheduler asyncScheduler = null)
    {
        _size = size;
        _taskFactory = taskFactory;
        _asyncScheduler = asyncScheduler;
    }

    public IFiber CreateAsyncFiber(Action<Exception> errorHandler) =>
        new Fiber(errorHandler, _size, _taskFactory, _asyncScheduler);
}
