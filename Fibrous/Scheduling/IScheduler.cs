using System;

namespace Fibrous.Scheduling
{
    public interface IScheduler
    {
        IDisposable Schedule(IFiber fiber, Action action, TimeSpan dueTime);
        IDisposable Schedule(IFiber fiber, Action action, TimeSpan startTime, TimeSpan interval);
    }
}