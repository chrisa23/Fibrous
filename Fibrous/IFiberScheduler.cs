namespace Fibrous
{
    using System;

    public interface IFiberScheduler
    {
        IDisposable Schedule(Fiber fiber, Action action, TimeSpan dueTime);
        IDisposable Schedule(Fiber fiber, Action action, TimeSpan startTime, TimeSpan interval);
    }
}