using System;

namespace Fibrous
{
    public interface IFiberScheduler
    {
        IDisposable Schedule(IFiber fiber, Action action, TimeSpan dueTime);
        IDisposable Schedule(IFiber fiber, Action action, TimeSpan startTime, TimeSpan interval);
    }
}
