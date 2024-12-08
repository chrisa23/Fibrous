using System;
using System.Threading.Tasks;

namespace Fibrous;

public interface IAsyncFiberScheduler
{
    IDisposable Schedule(IFiber fiber, Func<Task> action, TimeSpan dueTime);
    IDisposable Schedule(IFiber fiber, Func<Task> action, TimeSpan startTime, TimeSpan interval);
}
