using System;
using System.Threading.Tasks;

namespace Fibrous;

internal sealed class AsyncTimerScheduler : IAsyncFiberScheduler
{
    public IDisposable Schedule(IFiber fiber, Func<Task> action, TimeSpan dueTime)
    {
        if (dueTime.TotalMilliseconds <= 0)
        {
            AsyncPendingAction pending = new(action);
            fiber.Enqueue(pending.ExecuteAsync);
            return pending;
        }

        return new AsyncTimerAction(fiber, action, dueTime);
    }

    public IDisposable Schedule(IFiber fiber, Func<Task> action, TimeSpan dueTime, TimeSpan interval) =>
        new AsyncTimerAction(fiber, action, dueTime, interval);
}
