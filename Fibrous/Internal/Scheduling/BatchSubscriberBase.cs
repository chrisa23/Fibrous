using System;

namespace Fibrous;

internal abstract class BatchSubscriberBase<T> : IDisposable
{
    private readonly IDisposable _sub;
    protected readonly object BatchLock = new();
    protected readonly IFiber Fiber;
    protected readonly TimeSpan Interval;

    protected BatchSubscriberBase(ISubscriberPort<T> channel, IFiber fiber, TimeSpan interval)
    {
        _sub = channel.Subscribe(fiber, OnMessage);
        Fiber = fiber;
        Interval = interval;
    }

    public void Dispose() => _sub.Dispose();

    protected abstract void OnMessage(T msg);
}
