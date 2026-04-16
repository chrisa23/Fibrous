using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Channel that maintains its last value and immediately replays it to new subscribers.
/// </summary>
public sealed class StateChannel<T> : IChannel<T>, IInlineSubscriberPort<T>
{
    private readonly object _lock = new();
    private readonly Channel<T> _updateChannel = new();

    private bool _hasValue;
    private T _last;

    public StateChannel(T initial)
    {
        _last = initial;
        _hasValue = true;
    }

    public StateChannel()
    {
    }

    /// <summary>
    ///     Subscribes and immediately replays the current value when one exists.
    /// </summary>
    public IDisposable Subscribe(IFiber fiber, Func<T, Task> receive)
    {
        lock (_lock)
        {
            IDisposable disposable = _updateChannel.Subscribe(fiber, receive);
            if (_hasValue)
            {
                T item = _last;
                fiber.Enqueue(() => receive(item));
            }

            return disposable;
        }
    }

    public IDisposable Subscribe(IFiber fiber, Action<T> receive) =>
        Subscribe(fiber, receive.ToAsync());

    IDisposable IInlineSubscriberPort<T>.SubscribeInline(Action<T> receive)
    {
        lock (_lock)
        {
            IDisposable disposable = ((IInlineSubscriberPort<T>)_updateChannel)
                .SubscribeInline(receive);
            if (_hasValue)
            {
                T item = _last;
                receive(item);
            }

            return disposable;
        }
    }

    public void Publish(T message)
    {
        lock (_lock)
        {
            _last = message;
            _hasValue = true;
            _updateChannel.Publish(message);
        }
    }

    public void Dispose() => _updateChannel.Dispose();
}
