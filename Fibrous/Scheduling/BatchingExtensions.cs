using System;
using System.Collections.Generic;

namespace Fibrous.Scheduling
{
    public static class BatchingExtensions
    {
        public static IDisposable SubscribeToBatch<T>(this ISubscriberPort<T> port,
                                                      IFiber fiber,
                                                      Action<IList<T>> receive,
                                                      TimeSpan interval)
        {
            return new BatchSubscriber<T>(port, fiber, new TimerScheduler(), interval, receive);
        }


        public static IDisposable SubscribeToKeyedBatch<TKey, T>(this ISubscriberPort<T> port,
                                                                 IFiber fiber,
                                                                 Converter<T, TKey> keyResolver,
                                                                 Action<IDictionary<TKey, T>> receive,
                                                                 TimeSpan interval)
        {
            return new KeyedBatchSubscriber<TKey, T>(port, fiber, new TimerScheduler(),
                                                     interval, keyResolver, receive);
        }


        public static IDisposable SubscribeToLast<T>(this ISubscriberPort<T> port,
                                                     IFiber fiber,
                                                     Action<T> receive,
                                                     TimeSpan interval)
        {
            return new LastSubscriber<T>(port, fiber, new TimerScheduler(), interval, receive);
        }
    }
}