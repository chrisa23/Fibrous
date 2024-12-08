using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous;

internal sealed class AsyncKeyedBatchSubscriber<TKey, T>(
    ISubscriberPort<T> channel,
    IFiber fiber,
    TimeSpan interval,
    Converter<T, TKey> keyResolver,
    Func<IDictionary<TKey, T>, Task> target)
    : AsyncBatchSubscriberBase<T>(channel, fiber, interval)
{
    private Dictionary<TKey, T> _pending;

    protected override Task OnMessageAsync(T msg)
    {
        lock (BatchLock)
        {
            TKey key = keyResolver(msg);
            if (_pending == null)
            {
                _pending = new Dictionary<TKey, T>();
                Fiber.Schedule(FlushAsync, Interval);
            }

            _pending[key] = msg;
        }

        return Task.CompletedTask;
    }

    private Task FlushAsync()
    {
        IDictionary<TKey, T> toReturn = ClearPending();
        if (toReturn != null)
        {
            Fiber.Enqueue(() => target(toReturn));
        }

        return Task.CompletedTask;
    }

    private IDictionary<TKey, T> ClearPending()
    {
        lock (BatchLock)
        {
            if (_pending == null || _pending.Count == 0)
            {
                _pending = null;
                return null;
            }

            IDictionary<TKey, T> toReturn = _pending;
            _pending = null;
            return toReturn;
        }
    }
}
