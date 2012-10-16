using System;
using Fibrous.Utility;

namespace Fibrous.Scheduling
{
    public sealed class TimerScheduler : IScheduler
    {
        #region IScheduler Members

        public IDisposable Schedule(IFiber fiber, Action action, TimeSpan dueTime)
        {
            if (dueTime.TotalMilliseconds <= 0)
            {
                var pending = new PendingAction(action);
                fiber.Enqueue(pending.Execute);
                return pending;
            }
            return new TimerAction(fiber, action, dueTime);
        }

        public IDisposable Schedule(IFiber fiber, Action action, TimeSpan dueTime, TimeSpan interval)
        {
            return new TimerAction(fiber, action, dueTime, interval);
        }

        #endregion
    }
}