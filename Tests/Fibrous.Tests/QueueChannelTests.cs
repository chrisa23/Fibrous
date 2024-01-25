using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class QueueChannelTests
{
    private static void NOP(double durationMS)
    {
        double durationTicks = Math.Round(durationMS * Stopwatch.Frequency / 1000);
        Stopwatch sw = Stopwatch.StartNew();

        while (sw.ElapsedTicks < durationTicks)
        {
        }
    }

    private void QueueChannelTest(int fibers, Func<IAsyncFiber> factory, int messageCount,
        Func<IChannel<int>> channelFactory)
    {
        using Disposables queues = new();
        using AutoResetEvent reset = new(false);
        using IChannel<int> channel = channelFactory();
        int count = 0;

        async Task OnReceive(int obj)
        {
            int c = Interlocked.Increment(ref count);
            if (c == messageCount)
            {
                reset.Set();
            }

            NOP(1);
        }

        for (int i = 0; i < fibers; i++)
        {
            IAsyncFiber fiber = factory();
            queues.Add(fiber);
            channel.Subscribe(fiber, OnReceive);
        }

        Stopwatch sw = Stopwatch.StartNew();

        //Push messages
        for (int i = 1; i <= messageCount; i++)
        {
            channel.Publish(i);
        }

        Assert.IsTrue(reset.WaitOne(10000, false));
        sw.Stop();
        Console.WriteLine($"Fibers: {fibers}  End : {sw.ElapsedMilliseconds} Count {count}");
    }


    [Test]
    public void Multiple()
    {
        const int MessageCount = 100;
        List<IAsyncFiber> queues = new();
        int receiveCount = 0;
        using AutoResetEvent reset = new(false);
        object updateLock = new();

        async Task OnReceive(int obj)
        {
            Thread.Sleep(15);
            lock (updateLock)
            {
                Interlocked.Increment(ref receiveCount);
                if (receiveCount == MessageCount)
                {
                    reset.Set();
                }
            }
        }

        QueueChannel<int> channel = new();

        for (int i = 0; i < 5; i++)
        {
            AsyncFiber fiber = new();
            queues.Add(fiber);
            channel.Subscribe(fiber, OnReceive);
        }

        for (int i = 0; i < MessageCount; i++)
        {
            channel.Publish(i);
        }

        Assert.IsTrue(reset.WaitOne(10000, false));
        queues.ForEach(q => q.Dispose());
    }

    [Test]
    public void PoolQueue()
    {
        int msgCount = 1000;

        static IAsyncFiber Factory()
        {
            return new AsyncFiber();
        }

        Console.WriteLine("First");

        for (int i = 2; i < 10; i++)
        {
            QueueChannelTest(i, Factory, msgCount, () => new QueueChannel<int>());
        }
    }

    [Test]
    public void SingleConsumer()
    {
        int oneConsumed = 0;
        using AsyncFiber one = new();
        using AutoResetEvent reset = new(false);
        QueueChannel<int> channel = new();

        async Task OnMsg(int obj)
        {
            Interlocked.Increment(ref oneConsumed);
            if (oneConsumed == 20)
            {
                reset.Set();
            }
        }

        channel.Subscribe(one, OnMsg);
        for (int i = 0; i < 20; i++)
        {
            channel.Publish(i);
        }

        Assert.IsTrue(reset.WaitOne(10000, false));
    }

    [Test]
    public void SingleConsumerWithException()
    {
        using AutoResetEvent reset = new(false);

        async Task OnMsg(int num)
        {
            if (num == 0)
            {
                throw new Exception();
            }

            reset.Set();
        }

        List<Exception> failed = new();
        AsyncExceptionHandlingExecutor exec = new(async x => failed.Add(x));
        using AsyncFiber one = new(exec);
        QueueChannel<int> channel = new();
        channel.Subscribe(one, OnMsg);
        channel.Publish(0);
        channel.Publish(1);
        Assert.IsTrue(reset.WaitOne(10000, false));
        Assert.AreEqual(1, failed.Count);
    }

    [Test]
    public void FullDrain()
    {
        using AutoResetEvent reset = new(false);
        int count = 0;

        async Task OnMessage(int i)
        {
            int c = Interlocked.Increment(ref count);
            Thread.Sleep(100);
            if (c == 20)
            {
                reset.Set();
            }
        }

        using AsyncFiber fiber = new();
        using AsyncFiber fiber2 = new();
        using QueueChannel<int> queue = new();
        queue.Subscribe(fiber, OnMessage);
        queue.Subscribe(fiber2, OnMessage);
        for (int i = 0; i < 20; i++)
        {
            queue.Publish(i);
        }

        Assert.IsTrue(reset.WaitOne(15000, false));
        Assert.AreEqual(20, count);
    }

    [Test]
    public void FullDrain2()
    {
        const int Max = 1_000_000;
        using AutoResetEvent reset = new(false);
        int count = 0;

        async Task OnMessage(int i)
        {
            int c = Interlocked.Increment(ref count);
            if (c == Max)
            {
                reset.Set();
            }
        }

        using AsyncFiber fiber = new();
        using AsyncFiber fiber2 = new();
        using QueueChannel<int> queue = new();
        queue.Subscribe(fiber, OnMessage);
        queue.Subscribe(fiber2, OnMessage);
        for (int i = 0; i < Max; i++)
        {
            queue.Publish(i);
        }

        Assert.IsTrue(reset.WaitOne(15000, false));
        Assert.AreEqual(Max, count);
    }

    //[Test]
    //public void FullDrain3()
    //{
    //    const int Max = 1_000_000;
    //    using var reset = new AutoResetEvent(false);
    //    int count = 0;

    //    void OnMessage(int i)
    //    {
    //        int c = Interlocked.Increment(ref count);
    //        if (c == Max)
    //            reset.Set();
    //    }

    //    using var fiber = new Fiber();
    //    using var fiber2 = new Fiber();
    //    using var queue = new QueueChannelRR2<int>();
    //    queue.Subscribe(fiber, OnMessage);
    //    queue.Subscribe(fiber2, OnMessage);
    //    for (int i = 0; i < Max; i++)
    //    {
    //        queue.Publish(i);
    //    }

    //    Assert.IsTrue(reset.WaitOne(15000, false));
    //    Assert.AreEqual(Max, count);
    //}

    [Test]
    public void WorkDistribution()
    {
        int count = 0;
        int count2 = 0;

        async Task OnMessage(int i)
        {
            count++;
            Thread.Sleep(100);
        }

        async Task OnMessage2(int i)
        {
            count2++;
            Thread.Sleep(100);
        }

        using AsyncFiber fiber = new();
        using AsyncFiber fiber2 = new();
        using QueueChannel<int> queue = new();
        queue.Subscribe(fiber, OnMessage);
        queue.Subscribe(fiber2, OnMessage2);
        for (int i = 0; i < 20; i++)
        {
            queue.Publish(i);
        }

        Thread.Sleep(10000);
        Console.WriteLine($"{count} | {count2}");
        Assert.AreEqual(10, count);
        Assert.AreEqual(10, count2);
    }

    [Test]
    public void ThreeAsyncFibers()
    {
        int OperationsPerInvoke = 1000000;
        AutoResetEvent wait = new(false);
        int count = 0;

        Task AsyncHandler(int s)
        {
            int i = Interlocked.Increment(ref count);
            if (i == OperationsPerInvoke)
            {
                wait.Set();
            }

            return Task.CompletedTask;
        }

        using IChannel<int> _queue = new QueueChannel<int>();
        using AsyncFiber fiber1 = new();
        using AsyncFiber fiber2 = new();
        using AsyncFiber fiber3 = new();
        using IDisposable sub = _queue.Subscribe(fiber1, AsyncHandler);
        using IDisposable sub2 = _queue.Subscribe(fiber2, AsyncHandler);
        using IDisposable sub3 = _queue.Subscribe(fiber3, AsyncHandler);

        for (int j = 0; j < OperationsPerInvoke; j++)
        {
            _queue.Publish(j);
        }

        Assert.IsTrue(wait.WaitOne(15000, false));
    }
}
