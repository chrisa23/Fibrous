using System;

namespace Fibrous
{
    internal sealed class TimerScheduler : IFiberScheduler
    {
        public IDisposable Schedule(IFiber fiber, Action action, TimeSpan dueTime)
        {
            if (dueTime.TotalMilliseconds <= 0)
            {
                PendingAction pending = new PendingAction(action);
                fiber.Enqueue(pending.Execute);
                return pending;
            }

            return new TimerAction(fiber, action, dueTime);
        }

        public IDisposable Schedule(IFiber fiber, Action action, TimeSpan dueTime, TimeSpan interval) =>
            new TimerAction(fiber, action, dueTime, interval);
    }
}
