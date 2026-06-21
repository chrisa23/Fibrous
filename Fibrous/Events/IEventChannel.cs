using System;
using System.Threading.Tasks;

namespace Fibrous;

public interface IEventChannel : IEventTrigger, IEventPort
{
}

/// <summary>
///     Port for subscribing to event-style notifications.
/// </summary>
public interface IEventPort
{
    IDisposable Subscribe(IFiber fiber, Func<Task> receive);
    IDisposable Subscribe(IFiber fiber, Action receive);
}

public static class EventPortExtensions
{
    /// <summary>
    ///     Subscribes to an event port and delivers only the last event seen in each interval.
    /// </summary>
    public static IDisposable SubscribeThrottled(
        this IEventPort port,
        IFiber fiber,
        Action receive,
        TimeSpan span) =>
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
        void Handler() => fiber.Enqueue(receive);

        IDisposable disposable = _internalEvent.Subscribe(Handler);
        return new Unsubscriber(disposable, fiber);
    }

    public IDisposable Subscribe(IFiber fiber, Action receive) => Subscribe(fiber, receive.ToAsync());
}
