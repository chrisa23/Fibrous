namespace Fibrous
{
    using System;
    using System.Collections.Generic;
    using Fibrous.Fibers;
    using Fibrous.Scheduling;

    public interface ISubscribePort<out T>
    {
        /// <summary>
        /// Subscribe 
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        IDisposable Subscribe(IFiber fiber, Action<T> receive);
    }

    public static class SubscribePortExtensions
    {
        /// <summary>   Method that subscribe to a periodic batch. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="port">     The port to act on. </param>
        /// <param name="fiber">    The fiber. </param>
        /// <param name="receive">  The receive. </param>
        /// <param name="interval"> The interval. </param>
        /// <returns>   . </returns>
        public static IDisposable SubscribeToBatch<T>(this ISubscribePort<T> port,
                                                      IFiber fiber,
                                                      Action<T[]> receive,
                                                      TimeSpan interval)
        {
            return new BatchSubscriber<T>(port, fiber, interval, receive);
        }

        public static IDisposable SubscribeToKeyedBatch<TKey, T>(this ISubscribePort<T> port,
                                                                 IFiber fiber,
                                                                 Converter<T, TKey> keyResolver,
                                                                 Action<IDictionary<TKey, T>> receive,
                                                                 TimeSpan interval)
        {
            return new KeyedBatchSubscriber<TKey, T>(port, fiber, interval, keyResolver, receive);
        }

        public static IDisposable SubscribeToLast<T>(this ISubscribePort<T> port,
                                                     IFiber fiber,
                                                     Action<T> receive,
                                                     TimeSpan interval)
        {
            return new LastSubscriber<T>(port, fiber, interval, receive);
        }

        public static IDisposable Subscribe<T>(this ISubscribePort<T> port,
                                               IFiber fiber,
                                               Action<T> receive,
                                               Predicate<T> filter)
        {
            Action<T> filteredReceiver = x =>
            {
                if (filter(x))
                    fiber.Enqueue(() => receive(x));
            };
            return port.Subscribe(new StubFiber(), filteredReceiver);
        }
    }
}