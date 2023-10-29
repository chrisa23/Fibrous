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
    IDisposable Subscribe(IFiber fiber, Action receive);

    IDisposable Subscribe(IAsyncFiber fiber, Func<Task> receive);

    IDisposable Subscribe(Action receive);
}

public static class EventPortExtensions
{
    //throttle
    public static IDisposable
        SubscribeThrottled(this IEventPort port, IFiber fiber, Action receive, TimeSpan span) =>
        new LastEventSubscriber(port, fiber, span, receive);
}
