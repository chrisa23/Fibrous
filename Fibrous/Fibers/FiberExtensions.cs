namespace Fibrous
{
    using System;
    using System.ComponentModel;

    public static class FiberExtensions
    {

        /// <summary>
        /// Subscribe to a channel from the fiber.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fiber"></param>
        /// <param name="channel"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IDisposable Subscriber<T>(this IFiber fiber, ISubscriberPort<T> channel, Action<T> handler)
        {
            return channel.Subscribe(fiber, handler);
        }


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