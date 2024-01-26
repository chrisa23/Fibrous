using System;
using System.Threading.Tasks;

namespace Fibrous;

public interface IEventChannel : IEventTrigger, IEventPort
{
}

public interface IEventTrigger
{
    void Trigger();
}

public interface IEventPort
{
    IDisposable Subscribe(IFiber fiber, Func<Task> receive);
    IDisposable Subscribe(Action receive);
}

public static class EventPortExtensions
{
    public static IDisposable SubscribeThrottled(this IEventPort port, IFiber fiber, Action receive, TimeSpan span) =>
        new AsyncLastEventSubscriber(port, fiber, span, receive);
}

public class EventChannel : IEventChannel, IDisposable
{
    private readonly Event _internalEvent = new();

    internal bool HasSubscriptions => _internalEvent.HasSubscriptions;

    public void Dispose() => _internalEvent.Dispose();

    public void Trigger() => _internalEvent.Trigger();

    public IDisposable Subscribe(IFiber fiber, Func<Task> receive)
    {
        void Action() => fiber.Enqueue(receive);

        IDisposable disposable = _internalEvent.Subscribe(Action);
        return new Unsubscriber(disposable, fiber);
    }

    public IDisposable Subscribe(Action receive) => _internalEvent.Subscribe(receive);
}
