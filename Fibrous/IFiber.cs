using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous;

public interface IFiber : IScheduler, IDisposableRegistry
{
    void Enqueue(Action action);
    void Enqueue(Func<Task> action);
}

public static class FiberExtensions
{
    /// <summary>
    ///     Schedules an action using a cron expression.
    /// </summary>
    /// <param name="scheduler">Scheduler that owns the cron registration.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="cron">Cron expression in Quartz format.</param>
    public static IDisposable CronSchedule(this IScheduler scheduler, Func<Task> action, string cron) =>
        new CronScheduler(scheduler, action, cron);

    /// <summary>
    ///     Subscribe to a channel from the fiber.
    /// </summary>
    /// <param name="fiber">Fiber that receives the messages.</param>
    /// <param name="channel">Channel to subscribe to.</param>
    /// <param name="handler">Handler invoked for each message.</param>
    public static IDisposable Subscribe<T>(
        this IFiber fiber,
        ISubscriberPort<T> channel,
        Func<T, Task> handler) =>
        channel.Subscribe(fiber, handler);

    /// <summary>
    ///     Subscribes to a port and delivers messages in periodic batches.
    /// </summary>
    public static IDisposable SubscribeToBatch<T>(
        this IFiber fiber,
        ISubscriberPort<T> port,
        Func<T[], Task> receive,
        TimeSpan interval) =>
        new BatchSubscriber<T>(port, fiber, interval, receive);

    /// <summary>
    ///     Subscribes to periodic batches while retaining only the last item per key.
    /// </summary>
    public static IDisposable SubscribeToKeyedBatch<TKey, T>(
        this IFiber fiber,
        ISubscriberPort<T> port,
        Converter<T, TKey> keyResolver,
        Func<IDictionary<TKey, T>, Task> receive,
        TimeSpan interval) =>
        new KeyedBatchSubscriber<TKey, T>(port, fiber, interval, keyResolver, receive);

    /// <summary>
    ///     Subscribes to a port but only delivers the last message seen in each interval.
    /// </summary>
    public static IDisposable SubscribeToLast<T>(
        this IFiber fiber,
        ISubscriberPort<T> port,
        Func<T, Task> receive,
        TimeSpan interval) =>
        new LastSubscriber<T>(port, fiber, interval, receive);

    /// <summary>
    ///     Subscribes with a publisher-side predicate to avoid enqueuing discarded messages.
    /// </summary>
    public static IDisposable Subscribe<T>(
        this IFiber fiber,
        ISubscriberPort<T> port,
        Func<T, Task> receive,
        Predicate<T> filter)
    {
        if (port is not IInlineSubscriberPort<T> inlinePort)
        {
            throw new NotSupportedException("Publisher-side filtering requires an inline subscriber port.");
        }

        void FilteredReceiver(T x)
        {
            if (filter(x))
            {
                fiber.Enqueue(() => receive(x));
            }
        }

        // Filtering stays on the publisher thread to avoid enqueuing discarded messages.
        IDisposable sub = inlinePort.SubscribeInline(FilteredReceiver);
        return new Unsubscriber(sub, fiber);
    }

    /// <summary>
    ///     Creates a channel and immediately subscribes the fiber to it.
    /// </summary>
    public static IChannel<T> NewChannel<T>(this IFiber fiber, Action<T> onEvent)
    {
        Channel<T> channel = new();
        channel.Subscribe(fiber, onEvent);
        return channel;
    }

    /// <summary>
    ///     Creates a channel and immediately subscribes the fiber to it.
    /// </summary>
    public static IChannel<T> NewChannel<T>(this IFiber fiber, Func<T, Task> onEvent)
    {
        Channel<T> channel = new();
        channel.Subscribe(fiber, onEvent);
        return channel;
    }

    /// <summary>
    ///     Creates a request/reply channel and installs a request handler on the fiber.
    /// </summary>
    public static IRequestPort<TRq, TRp> NewRequestPort<TRq, TRp>(
        this IFiber fiber,
        Func<IRequest<TRq, TRp>, Task> onEvent)
    {
        RequestChannel<TRq, TRp> channel = new();
        channel.SetRequestHandler(fiber, onEvent);
        return channel;
    }
}
