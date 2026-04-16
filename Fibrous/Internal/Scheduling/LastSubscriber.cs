using System;
using System.Threading.Tasks;

namespace Fibrous;

internal sealed class LastSubscriber<T>(
    ISubscriberPort<T> channel,
    IFiber fiber,
    TimeSpan interval,
    Func<T, Task> target)
    : BatchSubscriberBase<T>(channel, fiber, interval)
{
    private bool _flushPending;
    private T _pending;

    protected override Task OnMessageAsync(T message)
    {
        lock (BatchLock)
        {
            if (!_flushPending)
            {
                Fiber.Schedule(FlushAsync, Interval);
                _flushPending = true;
            }

            _pending = message;
        }

        return Task.CompletedTask;
    }

    private Task FlushAsync()
    {
        T toReturn = ClearPending();
        Fiber.Enqueue(() => target(toReturn));
        return Task.CompletedTask;
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

internal sealed class AsyncLastEventSubscriber : IDisposable
{
    private readonly   Action      _target;
    private readonly   IDisposable _subscription;
    private readonly   object      _batchLock    = new();
    private readonly   IFiber      _fiber;
    private readonly   TimeSpan    _interval;
    private            bool        _flushPending;
    private            bool        _pending;

    public AsyncLastEventSubscriber(
        IEventPort channel,
        IFiber fiber,
        TimeSpan interval,
        Action target)
    {
        _subscription = channel.Subscribe(fiber, OnMessageAsync);
        _fiber        = fiber;
        _interval = interval;
        _target   = target;
    }

    private Task OnMessageAsync()
    {
        lock (_batchLock)
        {
            if (!_flushPending)
            {
                _fiber.Schedule(FlushAsync, _interval);
                _flushPending = true;
            }

            _pending = true;
        }

        return Task.CompletedTask;
    }

    private Task FlushAsync()
    {
        if (ClearPending())
        {
            _target();
        }

        return Task.CompletedTask;
    }

    private bool ClearPending()
    {
        lock (_batchLock)
        {
            _flushPending = false;
            bool clearPending = _pending;
            _pending = false;
            return clearPending;
        }
    }

    public void Dispose() => _subscription.Dispose();
}
