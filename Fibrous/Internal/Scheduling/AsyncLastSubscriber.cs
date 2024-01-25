using System;
using System.Threading.Tasks;

namespace Fibrous;

internal sealed class AsyncLastSubscriber<T> : AsyncBatchSubscriberBase<T>
{
    private readonly Func<T, Task> _target;
    private bool _flushPending;
    private T _pending;

    public AsyncLastSubscriber(ISubscriberPort<T> channel,
        IAsyncFiber fiber,
        TimeSpan interval,
        Func<T, Task> target)
        : base(channel, fiber, interval) =>
        _target = target;

    protected override Task OnMessageAsync(T msg)
    {
        lock (BatchLock)
        {
            if (!_flushPending)
            {
                Fiber.Schedule(FlushAsync, Interval);
                _flushPending = true;
            }

            _pending = msg;
        }

        return Task.CompletedTask;
    }

    private Task FlushAsync()
    {
        T toReturn = ClearPending();
        Fiber.Enqueue(() => _target(toReturn));
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
    private readonly   Action _target;
    private            bool        _flushPending;
    private            bool        _pending;
    private readonly   IDisposable _sub;
    protected readonly object      BatchLock = new();
    protected readonly IAsyncFiber Fiber;
    protected readonly TimeSpan    Interval;
    public AsyncLastEventSubscriber(IEventPort channel,
        IAsyncFiber fiber,
        TimeSpan interval,
        Action target)
    {
        _sub = channel.Subscribe(fiber, OnMessageAsync);
        Fiber = fiber;
        Interval = interval;
        _target = target;
    }

    protected Task OnMessageAsync()
    {
        lock (BatchLock)
        {
            if (!_flushPending)
            {
                Fiber.Schedule(FlushAsync, Interval);
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

        //Fiber.Enqueue(() => toReturn ? _target() : Task.CompletedTask);
        return Task.CompletedTask;
    }

    private bool ClearPending()
    {
        lock (BatchLock)
        {
            _flushPending = false;
            bool clearPending = _pending;
            _pending = false;
            return clearPending;
        }
    }

    public void Dispose() => _sub?.Dispose();
}
