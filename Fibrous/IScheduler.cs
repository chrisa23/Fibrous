namespace Fibrous
{
    using System;

    public interface IScheduler
    {
        IDisposable Schedule(Action action, TimeSpan dueTime);
        IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval);
    }
}