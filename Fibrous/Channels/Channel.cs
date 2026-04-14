using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Basic in-memory pub/sub channel.
/// </summary>
public sealed class Channel<T> : IChannel<T>, IInlineSubscriberPort<T>
{
    private readonly Event<T> _internalEvent = new();

    internal bool HasSubscriptions => _internalEvent.HasSubscriptions;

    public void Publish(T message) => _internalEvent.Publish(message);

    public IDisposable Subscribe(IFiber fiber, Func<T, Task> receive)
    {
        void Handler(T message) => fiber.Enqueue(() => receive(message));

        IDisposable disposable = _internalEvent.Subscribe(Handler);
        return new Unsubscriber(disposable, fiber);
    }

    public IDisposable Subscribe(IFiber fiber, Action<T> receive)
    {
        void Handler(T message) => fiber.Enqueue(() => receive(message));

        IDisposable disposable = _internalEvent.Subscribe(Handler);
        return new Unsubscriber(disposable, fiber);
    }

    IDisposable IInlineSubscriberPort<T>.SubscribeInline(Action<T> receive) => _internalEvent.Subscribe(receive);

    public void Dispose() => _internalEvent.Dispose();
}
