namespace Fibrous
{
    using System;

    public static class FiberExtensions
    {
        //new fibers...
        public static IPublisherPort<T> NewPublishPort<T>(this IFiber fiber, Action<T> onEvent)
        {
            var channel = new Channel<T>();
            channel.Subscribe(fiber, onEvent);
            return channel;
        }

        public static IRequestPort<Rq, Rp> NewRequestPort<Rq, Rp>(this IFiber fiber, Action<IRequest<Rq, Rp>> onEvent)
        {
            var channel = new RequestChannel<Rq, Rp>();
            channel.SetRequestHandler(fiber, onEvent);
            return channel;
        }
    }
}