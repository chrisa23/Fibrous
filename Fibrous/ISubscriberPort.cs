using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Port for subscribing to messages.
/// </summary>
public interface ISubscriberPort<out T>
{
    /// <summary>
///     Subscribe to messages on this channel with a fiber and handler.
    /// </summary>
    /// <param name="fiber">Fiber that receives the messages.</param>
    /// <param name="receive">Handler invoked for each message.</param>
    IDisposable Subscribe(IFiber fiber, Func<T, Task> receive);

    /// <summary>
///     Subscribe to messages on this channel with a fiber and handler.
    /// </summary>
    /// <param name="fiber">Fiber that receives the messages.</param>
    /// <param name="receive">Handler invoked for each message.</param>
    IDisposable Subscribe(IFiber fiber, Action<T> receive);
}

public static class SubscriberPortExtensions
{
    /// <summary>
    ///     Subscribes to a port and delivers messages in periodic batches.
    /// </summary>
    public static IDisposable SubscribeToBatch<T>(
        this ISubscriberPort<T> port,
        IFiber fiber,
        Func<T[], Task> receive,
        TimeSpan interval) =>
        new BatchSubscriber<T>(port, fiber, interval, receive);

    /// <summary>
    ///     Subscribes to periodic batches while retaining only the last item per key.
    /// </summary>
    public static IDisposable SubscribeToKeyedBatch<TKey, T>(
        this ISubscriberPort<T> port,
        IFiber fiber,
        Converter<T, TKey> keyResolver,
        Func<IDictionary<TKey, T>, Task> receive,
        TimeSpan interval) =>
        new KeyedBatchSubscriber<TKey, T>(port, fiber, interval, keyResolver, receive);

    /// <summary>
    ///     Subscribes to a port but only delivers the last message seen in each interval.
    /// </summary>
    public static IDisposable SubscribeToLast<T>(
        this ISubscriberPort<T> port,
        IFiber fiber,
        Func<T, Task> receive,
        TimeSpan interval) =>
        new LastSubscriber<T>(port, fiber, interval, receive);

    /// <summary>
    ///     Subscribes with a publisher-side predicate to avoid enqueuing discarded messages.
    /// </summary>
    public static IDisposable Subscribe<T>(
        this ISubscriberPort<T> port,
        IFiber fiber,
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

        IDisposable sub = inlinePort.SubscribeInline(FilteredReceiver);
        return new Unsubscriber(sub, fiber);
    }

    /// <summary>
    ///     Connects a subscriber port directly to a publisher port without an intermediate fiber.
    /// </summary>
    public static IDisposable Connect<T>(
        this ISubscriberPort<T> port,
        IPublisherPort<T> receive)
    {
        if (port is not IInlineSubscriberPort<T> inlinePort)
        {
            throw new NotSupportedException("Connecting without a fiber requires an inline subscriber port.");
        }

        return inlinePort.SubscribeInline(receive.Publish);
    }
}
