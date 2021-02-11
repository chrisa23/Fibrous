using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IAsyncFiberScheduler
    {
        IDisposable Schedule(IAsyncFiber fiber, Func<Task> action, TimeSpan dueTime);
        IDisposable Schedule(IAsyncFiber fiber, Func<Task> action, TimeSpan startTime, TimeSpan interval);
    }
}
