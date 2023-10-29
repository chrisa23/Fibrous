using System;
using System.Threading.Tasks;

namespace Fibrous;

public class EventChannel : IEventChannel, IDisposable
{
    private readonly Event _internalEvent = new();

    internal bool HasSubscriptions => _internalEvent.HasSubscriptions;

    public void Dispose() => _internalEvent.Dispose();

    public void Trigger() => _internalEvent.Trigger();

    public IDisposable Subscribe(IFiber fiber, Action receive)
    {
        void Action()
        {
            fiber.Enqueue(receive);
        }

        IDisposable disposable = _internalEvent.Subscribe(Action);
        return new Unsubscriber(disposable, fiber);
    }

    public IDisposable Subscribe(IAsyncFiber fiber, Func<Task> receive)
    {
        void Action()
        {
            fiber.Enqueue(receive);
        }

        IDisposable disposable = _internalEvent.Subscribe(Action);
        return new Unsubscriber(disposable, fiber);
    }


    public IDisposable Subscribe(Action receive) => _internalEvent.Subscribe(receive);
}
