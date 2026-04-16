using System;
using System.Threading.Tasks;

namespace Fibrous;

internal abstract class BatchSubscriberBase<T> : IDisposable
{
    private readonly   IDisposable _subscription;
    protected readonly object      BatchLock = new();
    protected readonly IFiber      Fiber;
    protected readonly TimeSpan Interval;

    protected BatchSubscriberBase(ISubscriberPort<T> channel, IFiber fiber, TimeSpan interval)
    {
        _subscription = channel.Subscribe(fiber, OnMessageAsync);
        Fiber         = fiber;
        Interval = interval;
    }

    public void Dispose() => _subscription.Dispose();

    protected abstract Task OnMessageAsync(T message);
}
