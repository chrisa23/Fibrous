using System;

namespace Fibrous;

internal sealed class LastSubscriber<T> : BatchSubscriberBase<T>
{
    private readonly Action<T> _target;
    private bool _flushPending;
    private T _pending;

    public LastSubscriber(ISubscriberPort<T> channel,
        IFiber fiber,
        TimeSpan interval,
        Action<T> target)
        : base(channel, fiber, interval) =>
        _target = target;

    protected override void OnMessage(T msg)
    {
        lock (BatchLock)
        {
            if (!_flushPending)
            {
                Fiber.Schedule(Flush, Interval);
                _flushPending = true;
            }

            _pending = msg;
        }
    }

    private void Flush()
    {
        T toReturn = ClearPending();
        Fiber.Enqueue(() => _target(toReturn));
    }

    private T ClearPending()
    {
        lock (BatchLock)
        {
            _flushPending = false;
            return _pending;
        }
    }
}
