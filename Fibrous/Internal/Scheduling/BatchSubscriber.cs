using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous;

internal sealed class BatchSubscriber<T>(
    ISubscriberPort<T> channel,
    IFiber fiber,
    TimeSpan interval,
    Func<T[], Task> receive)
    : BatchSubscriberBase<T>(channel, fiber, interval)
{
    private List<T> _pending;

    protected override Task OnMessageAsync(T msg)
    {
        lock (BatchLock)
        {
            if (_pending == null)
            {
                _pending = new List<T>();
                Fiber.Schedule(FlushAsync, Interval);
            }

            _pending.Add(msg);
        }

        return Task.CompletedTask;
    }

    private Task FlushAsync()
    {
        T[] toFlush = null;
        lock (BatchLock)
        {
            if (_pending != null)
            {
                toFlush = _pending.ToArray();
                _pending = null;
            }
        }

        if (toFlush != null)
        {
            Fiber.Enqueue(() => receive(toFlush));
        }

        return Task.CompletedTask;
    }
}
