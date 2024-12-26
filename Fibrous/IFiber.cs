using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous;

//It's suggested to always use an Exception callback with the IAsyncFiber
public interface IFiber : IScheduler, IDisposableRegistry
{
    void Enqueue(Action action);
    void Enqueue(Func<Task> action);
}

public interface IScheduler
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

    /// <summary>
    ///     Schedule an action to be executed once
    /// </summary>
    /// <param name="action"></param>
    /// <param name="dueTime"></param>
    /// <returns></returns>
    IDisposable Schedule(Action action, TimeSpan dueTime);

    /// <summary>
    ///     Schedule an action to be taken repeatedly
    /// </summary>
    /// <param name="action"></param>
    /// <param name="startTime"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval);
}

public static class AsyncFiberExtensions
{
    /// <summary>
    /// Schedule an action to run on a cron schedule
    /// </summary>
    /// <param name="scheduler"></param>
    /// <param name="action"></param>
    /// <param name="cron"></param>
    /// <returns></returns>
    public static IDisposable CronSchedule(this IScheduler scheduler, Func<Task> action, string cron) =>
        new AsyncCronScheduler(scheduler, action, cron);

    /// <summary>
    ///     Subscribe to a channel from the fiber.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fiber"></param>
    /// <param name="channel"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public static IDisposable Subscribe<T>(this IFiber fiber, ISubscriberPort<T> channel,
        Func<T, Task> handler) =>
        channel.Subscribe(fiber, handler);

    /// <summary>Method that subscribe to a periodic batch. </summary>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    /// <param name="port">     The port to act on. </param>
    /// <param name="fiber">    The fiber. </param>
    /// <param name="receive">  The receive. </param>
    /// <param name="interval"> The interval. </param>
    /// <returns>   . </returns>
    public static IDisposable SubscribeToBatch<T>(this IFiber fiber,
        ISubscriberPort<T> port,
        Func<T[], Task> receive,
        TimeSpan interval) =>
        new AsyncBatchSubscriber<T>(port, fiber, interval, receive);

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
    public static IDisposable SubscribeToKeyedBatch<TKey, T>(this IFiber fiber,
        ISubscriberPort<T> port,
        Converter<T, TKey> keyResolver,
        Func<IDictionary<TKey, T>, Task> receive,
        TimeSpan interval) =>
        new AsyncKeyedBatchSubscriber<TKey, T>(port, fiber, interval, keyResolver, receive);

    /// <summary>
    ///     Subscribe to a port but only consume the last msg per interval
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="port"></param>
    /// <param name="fiber"></param>
    /// <param name="receive"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static IDisposable SubscribeToLast<T>(this IFiber fiber,
        ISubscriberPort<T> port,
        Func<T, Task> receive,
        TimeSpan interval) =>
        new AsyncLastSubscriber<T>(port, fiber, interval, receive);

    /// <summary>
    ///     Subscribe with a message predicate to filter messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="port"></param>
    /// <param name="fiber"></param>
    /// <param name="receive"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static IDisposable Subscribe<T>(this IFiber fiber,
        ISubscriberPort<T> port,
        Func<T, Task> receive,
        Predicate<T> filter)
    {
        void FilteredReceiver(T x)
        {
            if (filter(x))
            {
                fiber.Enqueue(() => receive(x));
            }
        }

        //we use a stub fiber to force the filtering onto the publisher thread.
        IDisposable sub = port.Subscribe(FilteredReceiver);
        return new Unsubscriber(sub, fiber);
    }

    public static IChannel<T> NewChannel<T>(this IFiber fiber, Action<T> onEvent)
    {
        Channel<T> channel = new();
        channel.Subscribe(fiber, onEvent);
        return channel;
    }
    public static IChannel<T> NewChannel<T>(this IFiber fiber, Func<T, Task> onEvent)
    {
        Channel<T> channel = new();
        channel.Subscribe(fiber, onEvent);
        return channel;
    }

    public static IRequestPort<TRq, TRp> NewRequestPort<TRq, TRp>(this IFiber fiber,
        Func<IRequest<TRq, TRp>, Task> onEvent)
    {
        RequestChannel<TRq, TRp> channel = new();
        channel.SetRequestHandler(fiber, onEvent);
        return channel;
    }
}
