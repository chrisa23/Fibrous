using System;
using System.Threading.Tasks;

namespace Fibrous;

public sealed class    Channel<T> : IChannel<T>
{
    private readonly Event<T> _internalEvent = new();

    internal bool HasSubscriptions => _internalEvent.HasSubscriptions;

    public void Publish(T msg) => _internalEvent.Publish(msg);

    public IDisposable Subscribe(IFiber fiber, Func<T, Task> receive)
    {
        void Receive(T msg) => fiber.Enqueue(() => receive(msg));

        IDisposable disposable = _internalEvent.Subscribe(Receive);
        return new Unsubscriber(disposable, fiber);
    }

    public IDisposable Subscribe(IFiber fiber, Action<T> receive)
    {
        void Receive(T msg) => fiber.Enqueue(() => receive(msg));

        IDisposable disposable = _internalEvent.Subscribe(Receive);
        return new Unsubscriber(disposable, fiber);
    }


    public IDisposable Subscribe(Action<T> receive) => _internalEvent.Subscribe(receive);

    public void Dispose() => _internalEvent.Dispose();
}
