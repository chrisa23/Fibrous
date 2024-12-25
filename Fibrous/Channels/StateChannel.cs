using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Channel that maintains its last value which is passed to new subscribers.  Useful with Enums or values representing
///     latest status.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class StateChannel<T> : IChannel<T>
{
    private readonly object _lock = new();
    private readonly IChannel<T> _updateChannel = new Channel<T>();
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

    public IDisposable Subscribe(Action<T> receive)
    {
        lock (_lock)
        {
            IDisposable disposable = _updateChannel.Subscribe(receive);
            if (_hasValue)
            {
                T item = _last;
                receive(item);
            }

            return disposable;
        }
    }

    public void Publish(T msg)
    {
        lock (_lock)
        {
            _last = msg;
            _hasValue = true;
            _updateChannel.Publish(msg);
        }
    }

    public void Dispose() => _updateChannel.Dispose();
}
