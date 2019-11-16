using System;
using System.Threading.Tasks;

namespace Fibrous
{
    internal sealed class AsyncTimerScheduler : IAsyncFiberScheduler
    {
        public IDisposable Schedule(IAsyncFiber fiber, Func<Task> action, TimeSpan dueTime)
        {
            if (dueTime.TotalMilliseconds <= 0)
            {
                var pending = new AsyncPendingAction(action);
                fiber.Enqueue(pending.Execute);
                return pending;
            }

            return new AsyncTimerAction(fiber, action, dueTime);
        }

        public IDisposable Schedule(IAsyncFiber fiber, Func<Task> action, TimeSpan dueTime, TimeSpan interval)
        {
            return new AsyncTimerAction(fiber, action, dueTime, interval);
        }
    }
}