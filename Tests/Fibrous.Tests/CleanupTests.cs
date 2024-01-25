using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class CleanupTests
{
    [Test]
    public void AsyncChannelSubscription()
    {
        RunTest(new AsyncStubFiber(), new Channel<int>(), (f, c) => f.Subscribe(c, ReceiveAsync));
        RunTest(new AsyncFiber(), new Channel<int>(), (f, c) => f.Subscribe(c, ReceiveAsync));
        RunTest(new AsyncStubFiber(), new Channel<int>(), (f, c) => f.Subscribe(c, ReceiveAsync, _true));
        RunTest(new AsyncFiber(), new Channel<int>(), (f, c) => f.Subscribe(c, ReceiveAsync, _true));
        RunTest(new AsyncStubFiber(), new Channel<int>(),
            (f, c) => f.SubscribeToBatch(c, ReceiveAsync, _oneSecond));
        RunTest(new AsyncFiber(), new Channel<int>(), (f, c) => f.SubscribeToBatch(c, ReceiveAsync, _oneSecond));
        RunTest(new AsyncStubFiber(), new Channel<int>(), (f, c) => f.SubscribeToLast(c, ReceiveAsync, _oneSecond));
        RunTest(new AsyncFiber(), new Channel<int>(), (f, c) => f.SubscribeToLast(c, ReceiveAsync, _oneSecond));
        RunTest(new AsyncStubFiber(), new Channel<int>(),
            (f, c) => f.SubscribeToKeyedBatch(c, x => x, ReceiveAsync, _oneSecond));
        RunTest(new AsyncFiber(), new Channel<int>(),
            (f, c) => f.SubscribeToKeyedBatch(c, x => x, ReceiveAsync, _oneSecond));
    }

    [Test]
    public void AsyncEventChannelSubscription()
    {
        RunTest(new AsyncStubFiber(), new EventChannel(), (f, c) => c.Subscribe(f, () => Task.CompletedTask));
        RunTest(new AsyncFiber(), new EventChannel(), (f, c) => c.Subscribe(f, () => Task.CompletedTask));
    }

    [Test]
    public void AsyncEventBusSubscription()
    {
        RunTest(new AsyncStubFiber(), f => EventBus<int>.Subscribe(f, ReceiveAsync));
        RunTest(new AsyncFiber(), f => EventBus<int>.Subscribe(f, ReceiveAsync));
        RunTest(new AsyncStubFiber(), f => EventBus<int>.Subscribe(f, ReceiveAsync, _true));
        RunTest(new AsyncFiber(), f => EventBus<int>.Subscribe(f, ReceiveAsync, _true));
        RunTest(new AsyncStubFiber(), f => EventBus<int>.SubscribeToBatch(f, ReceiveAsync, _oneSecond));
        RunTest(new AsyncFiber(), f => EventBus<int>.SubscribeToBatch(f, ReceiveAsync, _oneSecond));
        RunTest(new AsyncStubFiber(), f => EventBus<int>.SubscribeToLast(f, ReceiveAsync, _oneSecond));
        RunTest(new AsyncFiber(), f => EventBus<int>.SubscribeToLast(f, ReceiveAsync, _oneSecond));
        RunTest(new AsyncStubFiber(),
            f => EventBus<int>.SubscribeToKeyedBatch(f, x => x, ReceiveAsync, _oneSecond));
        RunTest(new AsyncFiber(), f => EventBus<int>.SubscribeToKeyedBatch(f, x => x, ReceiveAsync, _oneSecond));
    }

    private readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
    private readonly Predicate<int> _true = x => true;

    private static void Receive(int i)
    {
    }

    private static void Receive(int[] i)
    {
    }

    private static void Receive(IDictionary<int, int> i)
    {
    }

    private static Task ReceiveAsync(int i) => Task.CompletedTask;
    private static Task ReceiveAsync(int[] i) => Task.CompletedTask;
    private static Task ReceiveAsync(IDictionary<int, int> i) => Task.CompletedTask;
    public static void RunTest(IAsyncFiber fiber, Channel<int> channel,
        Func<IAsyncFiber, Channel<int>, IDisposable> f) =>
        RunTest(fiber, () => f(fiber, channel), () => channel.HasSubscriptions);

    public static void RunTest(IAsyncFiber fiber, EventChannel channel,
        Func<IAsyncFiber, EventChannel, IDisposable> f) =>
        RunTest(fiber, () => f(fiber, channel), () => channel.HasSubscriptions);

    public static void RunTest(IAsyncFiber fiber, Func<IAsyncFiber, IDisposable> f) =>
        RunTest(fiber, () => f(fiber), () => EventBus<int>.Channel.HasSubscriptions);

    public static void RunTest(IDisposable fiber, Func<IDisposable> subscribe, Func<bool> hasSubs)
    {
        Assert.IsFalse(hasSubs());
        IDisposable sub = subscribe();
        Assert.IsTrue(hasSubs());
        sub.Dispose();
        Assert.IsFalse(hasSubs());
        subscribe();
        Assert.IsTrue(hasSubs());
        fiber.Dispose();
        Assert.IsFalse(hasSubs());
    }
}
