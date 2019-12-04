using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Port for subscribing to messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISubscriberPort<out T>
    {
        /// <summary>
        ///     Subscribe to messages on this channel with a fiber and handler.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        IDisposable Subscribe(IFiber fiber, Action<T> receive);

        IDisposable Subscribe(IAsyncFiber fiber, Func<T, Task> receive);
    }


    public static class SubscriberPortExtensions
    {
        /// <summary>Method that subscribe to a periodic batch. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="port">     The port to act on. </param>
        /// <param name="fiber">    The fiber. </param>
        /// <param name="receive">  The receive. </param>
        /// <param name="interval"> The interval. </param>
        /// <returns>   . </returns>
        public static IDisposable SubscribeToBatch<T>(this ISubscriberPort<T> port,
            IAsyncFiber fiber,
            Func<T[], Task> receive,
            TimeSpan interval)
        {
            return new AsyncBatchSubscriber<T>(port, fiber, interval, receive);
        }

        /// <summary>Method that subscribe to a periodic batch. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="port">     The port to act on. </param>
        /// <param name="fiber">    The fiber. </param>
        /// <param name="receive">  The receive. </param>
        /// <param name="interval"> The interval. </param>
        /// <returns>   . </returns>
        public static IDisposable SubscribeToBatch<T>(this ISubscriberPort<T> port,
            IFiber fiber,
            Action<T[]> receive,
            TimeSpan interval)
        {
            return new BatchSubscriber<T>(port, fiber, interval, receive);
        }

        /// <summary>
        ///     Subscribe to a periodic batch, maintaining the last item by key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="fiber"></param>
        /// <param name="keyResolver"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToKeyedBatch<TKey, T>(this ISubscriberPort<T> port,
            IFiber fiber,
            Converter<T, TKey> keyResolver,
            Action<IDictionary<TKey, T>> receive,
            TimeSpan interval)
        {
            return new KeyedBatchSubscriber<TKey, T>(port, fiber, interval, keyResolver, receive);
        }

        /// <summary>
        ///     Subscribe to a periodic batch, maintaining the last item by key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="fiber"></param>
        /// <param name="keyResolver"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToKeyedBatch<TKey, T>(this ISubscriberPort<T> port,
            IAsyncFiber fiber,
            Converter<T, TKey> keyResolver,
            Func<IDictionary<TKey, T>, Task> receive,
            TimeSpan interval)
        {
            return new AsyncKeyedBatchSubscriber<TKey, T>(port, fiber, interval, keyResolver, receive);
        }

        /// <summary>
        ///     Subscribe to a port but only consume the last msg per interval
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToLast<T>(this ISubscriberPort<T> port,
            IFiber fiber,
            Action<T> receive,
            TimeSpan interval)
        {
            return new LastSubscriber<T>(port, fiber, interval, receive);
        }

        /// <summary>
        ///     Subscribe to a port but only consume the last msg per interval
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToLast<T>(this ISubscriberPort<T> port,
            IAsyncFiber fiber,
            Func<T, Task> receive,
            TimeSpan interval)
        {
            return new AsyncLastSubscriber<T>(port, fiber, interval, receive);
        }

        /// <summary>
        ///     Subscribe with a message predicate to filter messages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IDisposable Subscribe<T>(this ISubscriberPort<T> port,
            IFiber fiber,
            Action<T> receive,
            Predicate<T> filter)
        {
            void FilteredReceiver(T x)
            {
                if (filter(x))
                    fiber.Enqueue(() => receive(x));
            }

            //we use a stub fiber to force the filtering onto the publisher thread.
            var stub = StubFiber.StartNew();
            port.Subscribe(stub, FilteredReceiver);
            //We return the fiber which is disposable and contains the subscription
            return new Unsubscriber(stub, fiber);
        }

        /// <summary>
        ///     Subscribe with a message predicate to filter messages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IDisposable Subscribe<T>(this ISubscriberPort<T> port,
            IAsyncFiber fiber,
            Func<T, Task> receive,
            Predicate<T> filter)
        {
            void FilteredReceiver(T x)
            {
                if (filter(x))
                    fiber.Enqueue(() => receive(x));
            }

            //we use a stub fiber to force the filtering onto the publisher thread.
            var stub = StubFiber.StartNew();
            port.Subscribe(stub, FilteredReceiver);
            //We return the fiber which is disosable and contains the subscription
            return new Unsubscriber(stub, fiber);
        }

        /// <summary>
        ///     Quick way to connect a Subscriber port to a Publisher port.  Useful connecting channels and Agents
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public static IDisposable Connect<T>(this ISubscriberPort<T> port,
            IPublisherPort<T> receive)
        {
            var stub = StubFiber.StartNew();
            port.Subscribe(stub, receive.Publish);
            return stub;
        }
    }
}