using System;

namespace Fibrous.Scheduling
{
    public interface IScheduler
    {
        IDisposable Schedule(IFiber fiber, Action action, long firstInMs);
        IDisposable ScheduleOnInterval(IFiber fiber, Action action, long firstInMs, long regularInMs);
    }
}