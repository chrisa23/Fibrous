using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous;

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
    IDisposable Subscribe(IFiber fiber, Func<T, Task> receive);
    /// <summary>
    ///     Subscribe to messages on this channel with a fiber and handler.
    /// </summary>
    /// <param name="fiber"></param>
    /// <param name="receive"></param>
    /// <returns></returns>
    IDisposable Subscribe(IFiber fiber, Action<T> receive);
    /// <summary>
    ///     Subscribe to messages on this channel with a  handler directly.
    /// </summary>
    /// <param name="receive"></param>
    /// <returns></returns>
    IDisposable Subscribe(Action<T> receive);
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
        IFiber fiber,
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
    public static IDisposable SubscribeToKeyedBatch<TKey, T>(this ISubscriberPort<T> port,
        IFiber fiber,
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
    public static IDisposable SubscribeToLast<T>(this ISubscriberPort<T> port,
        IFiber fiber,
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
    public static IDisposable Subscribe<T>(this ISubscriberPort<T> port,
        IFiber fiber,
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

        IDisposable sub = port.Subscribe(FilteredReceiver);
        return new Unsubscriber(sub, fiber);
    }

    /// <summary>
    ///     Quick way to connect a Subscriber port to a Publisher port.  Useful connecting channels and Agents
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="port"></param>
    /// <param name="receive"></param>
    /// <returns></returns>
    public static IDisposable Connect<T>(this ISubscriberPort<T> port,
        IPublisherPort<T> receive) =>
        port.Subscribe(receive.Publish);
}
