namespace Fibrous
{
    using System;
    using System.Collections.Generic;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using Fibrous.Scheduling;

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
        public static IDisposable Subscribe<T>(this IFiber fiber, ISubscriberPort<T> channel, Action<T> handler)
        {
            return channel.Subscribe(fiber, handler);
        }

        /// <summary>Method that subscribe to a periodic batch. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="port">     The port to act on. </param>
        /// <param name="fiber">    The fiber. </param>
        /// <param name="receive">  The receive. </param>
        /// <param name="interval"> The interval. </param>
        /// <returns>   . </returns>
        public static IDisposable SubscribeToBatch<T>(this IFiber fiber,
            ISubscriberPort<T> port,
            Action<T[]> receive,
            TimeSpan interval)
        {
            return new BatchSubscriber<T>(port, fiber, interval, receive);
        }

        /// <summary>
        /// Subscribe to a periodic batch, maintaining the last item by key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="fiber"></param>
        /// <param name="keyResolver"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToKeyedBatch<TKey, T>(this IFiber fiber,
            ISubscriberPort<T> port,
            Converter<T, TKey> keyResolver,
            Action<IDictionary<TKey, T>> receive,
            TimeSpan interval)
        {
            return new KeyedBatchSubscriber<TKey, T>(port, fiber, interval, keyResolver, receive);
        }

        /// <summary>
        /// Subscribe to a port but only consume the last msg per interval
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToLast<T>(this IFiber fiber,
            ISubscriberPort<T> port,
            Action<T> receive,
            TimeSpan interval)
        {
            return new LastSubscriber<T>(port, fiber, interval, receive);
        }

        /// <summary>
        /// Subscribe with a message predicate to filter messages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IDisposable Subscribe<T>(this IFiber fiber,
            ISubscriberPort<T> port,
            Action<T> receive,
            Predicate<T> filter)
        {
            void FilteredReceiver(T x)
            {
                if (filter(x))
                    fiber.Enqueue(() => receive(x));
            }

            //we use a stub fiber to force the filtering onto the publisher thread.
            return port.Subscribe(StubFiber.StartNew(), FilteredReceiver);
        }

        public static IPublisherPort<T> NewPublishPort<T>(this IFiber fiber, Action<T> onEvent)
        {
            var channel = new Channel<T>();
            channel.Subscribe(fiber, onEvent);
            return channel;
        }

        public static IRequestPort<TRq, TRp> NewRequestPort<TRq, TRp>(this IFiber fiber, Action<IRequest<TRq, TRp>> onEvent)
        {
            var channel = new RequestChannel<TRq, TRp>();
            channel.SetRequestHandler(fiber, onEvent);
            return channel;
        }
    }
}