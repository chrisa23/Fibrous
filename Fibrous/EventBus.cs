﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Simple singleton point of event publishing by type.
/// </summary>
/// <typeparam name="T"></typeparam>
public static class EventBus<T>
{
    internal static readonly Channel<T> Channel = new();

    public static void Publish(T msg) => Channel.Publish(msg);

    public static IDisposable Subscribe(IFiber fiber, Func<T, Task> receive) =>
        Channel.Subscribe(fiber, receive);

    public static IDisposable Subscribe(IFiber fiber, Func<T, Task> receive, Predicate<T> filter) =>
        Channel.Subscribe(fiber, receive, filter);

    public static IDisposable SubscribeToBatch(IFiber fiber,
        Func<T[], Task> receive,
        TimeSpan interval) =>
        Channel.SubscribeToBatch(fiber, receive, interval);

    public static IDisposable SubscribeToKeyedBatch<TKey>(IFiber fiber,
        Converter<T, TKey> keyResolver,
        Func<IDictionary<TKey, T>, Task> receive,
        TimeSpan interval) =>
        Channel.SubscribeToKeyedBatch(fiber, keyResolver, receive, interval);

    public static IDisposable SubscribeToLast(IFiber fiber,
        Func<T, Task> receive,
        TimeSpan interval) =>
        Channel.SubscribeToLast(fiber, receive, interval);

    public static IDisposable Subscribe(IFiber fiber, Action<T> receive) =>
        Channel.Subscribe(fiber, receive);

    public static IDisposable Subscribe(IFiber fiber, Action<T> receive, Predicate<T> filter) =>
        Subscribe(fiber, receive.ToAsync(), filter);

    public static IDisposable SubscribeToBatch(IFiber fiber,
        Action<T[]> receive,
        TimeSpan interval) =>
        SubscribeToBatch(fiber, receive.ToAsync(), interval);

    public static IDisposable SubscribeToKeyedBatch<TKey>(IFiber fiber,
        Converter<T, TKey> keyResolver,
        Action<IDictionary<TKey, T>> receive,
        TimeSpan interval) =>
        SubscribeToKeyedBatch(fiber, keyResolver,receive.ToAsync(), interval);

    public static IDisposable SubscribeToLast(IFiber fiber,
        Action<T> receive,
        TimeSpan interval) =>
        SubscribeToLast(fiber, receive.ToAsync(), interval);
}
