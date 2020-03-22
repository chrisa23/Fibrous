using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IEventPort
    {
        IDisposable Subscribe(IFiber fiber, Action receive);

        IDisposable Subscribe(IAsyncFiber fiber, Func<Task> receive);

        IDisposable Subscribe(Action receive);
    }

    public static class EventPortExtensions
    {
        //throttle
        public static IDisposable SubscribeThrottled(this IEventPort port, IFiber fiber, Action receive, TimeSpan span)
        {
            var channel = new Channel<object>();
            var sub = channel.SubscribeToLast(fiber, x => receive(), span);
            var eventSub = port.Subscribe(() => channel.Publish(null));
            var dispose = new Disposables(new[] {channel, sub, eventSub});
            return new Unsubscriber(dispose, fiber);
        }
    }
}