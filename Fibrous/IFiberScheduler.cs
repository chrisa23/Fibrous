using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IFiberScheduler
    {
        IDisposable Schedule(IFiber fiber, Action action, TimeSpan dueTime);
        IDisposable Schedule(IFiber fiber, Action action, TimeSpan startTime, TimeSpan interval);
    }


    public interface IAsyncFiberScheduler
    {
        IDisposable Schedule(IAsyncFiber fiber, Func<Task> action, TimeSpan dueTime);
        IDisposable Schedule(IAsyncFiber fiber, Func<Task> action, TimeSpan startTime, TimeSpan interval);
    }
}