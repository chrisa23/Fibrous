using System;
using System.Threading.Tasks;

namespace Fibrous;

internal sealed class TimerScheduler : IFiberScheduler
{
    public IDisposable Schedule(IFiber fiber, Func<Task> action, TimeSpan dueTime)
    {
        if (dueTime.TotalMilliseconds <= 0)
        {
            PendingAction pending = new(action);
            fiber.Enqueue(pending.ExecuteAsync);
            return pending;
        }

        return new TimerAction(fiber, action, dueTime);
    }

    public IDisposable Schedule(IFiber fiber, Func<Task> action, TimeSpan dueTime, TimeSpan interval) =>
        new TimerAction(fiber, action, dueTime, interval);
}
