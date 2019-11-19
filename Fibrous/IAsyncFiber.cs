using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IAsyncFiber : IAsyncExecutionContext, IAsyncScheduler, IDisposableRegistry
    {
        /// <summary>
        ///     Start the fiber's queue
        /// </summary>
        /// <returns></returns>
        IAsyncFiber Start();

        /// <summary>
        ///     Stop the fiber
        /// </summary>
        void Stop();
    }
    
    public interface IAsyncExecutionContext
    {
        void Enqueue(Func<Task> action);
    }

    public interface IAsyncScheduler
    {
        /// <summary>
        ///     Schedule an action to be executed once
        /// </summary>
        /// <param name="action"></param>
        /// <param name="dueTime"></param>
        /// <returns></returns>
        IDisposable Schedule(Func<Task> action, TimeSpan dueTime);

        /// <summary>
        ///     Schedule an action to be taken repeatedly
        /// </summary>
        /// <param name="action"></param>
        /// <param name="startTime"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval);
    }

    public static class AsyncSchedulerExtensions
    {
        /// <summary>
        ///     Schedule an action at a DateTime
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="action"></param>
        /// <param name="when"></param>
        /// <returns></returns>
        public static IDisposable Schedule(this IAsyncScheduler scheduler, Func<Task> action, DateTime when)
        {
            return scheduler.Schedule(action, when - DateTime.Now);
        }

        /// <summary>
        ///     Schedule an action at a DateTime with an interval
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="action"></param>
        /// <param name="when"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IDisposable Schedule(this IAsyncScheduler scheduler, Func<Task> action, DateTime when, TimeSpan interval)
        {
            return scheduler.Schedule(action, when - DateTime.Now, interval);
        }
        

        public static IDisposable CronSchedule(this IAsyncScheduler scheduler, Func<Task> action, string cron)
        {
            return new AsyncCronScheduler(scheduler, action, cron);
        }
    }

    public static class AsyncFiberExtensions
    {
        /// <summary>
        ///     Subscribe to a channel from the fiber.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fiber"></param>
        /// <param name="channel"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IDisposable Subscribe<T>(this IAsyncFiber fiber, ISubscriberPort<T> channel,
            Func<T, Task> handler)
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
        public static IDisposable SubscribeToBatch<T>(this IAsyncFiber fiber,
            ISubscriberPort<T> port,
            Func<T[], Task> receive,
            TimeSpan interval)
        {
            return new AsyncBatchSubscriber<T>(port, fiber, interval, receive);
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
        public static IDisposable SubscribeToKeyedBatch<TKey, T>(this IAsyncFiber fiber,
            ISubscriberPort<T> port,
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
        public static IDisposable SubscribeToLast<T>(this IAsyncFiber fiber,
            ISubscriberPort<T> port,
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
        public static IDisposable Subscribe<T>(this IAsyncFiber fiber,
            ISubscriberPort<T> port,
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
            return stub;
        }

        public static IPublisherPort<T> NewPublishPort<T>(this IAsyncFiber fiber, Func<T, Task> onEvent)
        {
            var channel = new Channel<T>();
            channel.Subscribe(fiber, onEvent);
            return channel;
        }

        public static IRequestPort<TRq, TRp> NewRequestPort<TRq, TRp>(this IAsyncFiber fiber,
            Func<IRequest<TRq, TRp>, Task> onEvent)
        {
            var channel = new RequestChannel<TRq, TRp>();
            channel.SetRequestHandler(fiber, onEvent);
            return channel;
        }
    }
}