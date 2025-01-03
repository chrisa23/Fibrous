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
    private static void NOP(double durationMs)
    {
        double durationTicks = Math.Round(durationMs * Stopwatch.Frequency / 1000);
        Stopwatch sw = Stopwatch.StartNew();

        while (sw.ElapsedTicks < durationTicks)
        {
        }
    }

    private void QueueChannelTest(int fibers, Func<IFiber> factory, int messageCount,
        Func<IChannel<int>> channelFactory)
    {
        using Disposables queues = new();
        using AutoResetEvent reset = new(false);
        using IChannel<int> channel = channelFactory();
        int count = 0;

        Task OnReceive(int obj)
        {
            int c = Interlocked.Increment(ref count);
            if (c == messageCount)
            {
                // ReSharper disable once AccessToDisposedClosure
                reset.Set();
            }

            NOP(1);
            return Task.CompletedTask;
        }

        for (int i = 0; i < fibers; i++)
        {
            IFiber fiber = factory();
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
        const int messageCount = 100;
        List<IFiber> queues = new();
        int receiveCount = 0;
        using AutoResetEvent reset = new(false);
        object updateLock = new();

        Task OnReceive(int obj)
        {
            Thread.Sleep(15);
            lock (updateLock)
            {
                Interlocked.Increment(ref receiveCount);
                if (receiveCount == messageCount)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    reset.Set();
                }
            }

            return Task.CompletedTask;
        }

        QueueChannel<int> channel = new();

        for (int i = 0; i < 5; i++)
        {
            Fiber fiber = new();
            queues.Add(fiber);
            channel.Subscribe(fiber, OnReceive);
        }

        for (int i = 0; i < messageCount; i++)
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

        static IFiber Factory()
        {
            return new Fiber();
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
        using Fiber one = new();
        using AutoResetEvent reset = new(false);
        QueueChannel<int> channel = new();

        Task OnMsg(int obj)
        {
            Interlocked.Increment(ref oneConsumed);
            if (oneConsumed == 20)
            {
                // ReSharper disable once AccessToDisposedClosure
                reset.Set();
            }

            return Task.CompletedTask;
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

        Task OnMsg(int num)
        {
            if (num == 0)
            {
                throw new Exception();
            }

            // ReSharper disable once AccessToDisposedClosure
            reset.Set();
            return Task.CompletedTask;
        }

        List<Exception> failed = new();
        ExceptionHandlingExecutor exec = new(x => failed.Add(x));
        using Fiber one = new(exec);
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

        Task OnMessage(int i)
        {
            int c = Interlocked.Increment(ref count);
            Thread.Sleep(100);
            if (c == 20)
            {
                // ReSharper disable once AccessToDisposedClosure
                reset.Set();
            }

            return Task.CompletedTask;
        }

        using Fiber fiber = new();
        using Fiber fiber2 = new();
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
        const int max = 1_000_000;
        using AutoResetEvent reset = new(false);
        int count = 0;

        Task OnMessage(int i)
        {
            int c = Interlocked.Increment(ref count);
            if (c == max)
            {
                // ReSharper disable once AccessToDisposedClosure
                reset.Set();
            }

            return Task.CompletedTask;
        }

        using Fiber fiber = new();
        using Fiber fiber2 = new();
        using QueueChannel<int> queue = new();
        queue.Subscribe(fiber, OnMessage);
        queue.Subscribe(fiber2, OnMessage);
        for (int i = 0; i < max; i++)
        {
            queue.Publish(i);
        }

        Assert.IsTrue(reset.WaitOne(15000, false));
        Assert.AreEqual(max, count);
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

        Task OnMessage(int i)
        {
            count++;
            Thread.Sleep(100);
            return Task.CompletedTask;
        }

        Task OnMessage2(int i)
        {
            count2++;
            Thread.Sleep(100);
            return Task.CompletedTask;
        }

        using Fiber fiber = new();
        using Fiber fiber2 = new();
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
        int operationsPerInvoke = 1000000;
        AutoResetEvent wait = new(false);
        int count = 0;

        Task AsyncHandler(int s)
        {
            int i = Interlocked.Increment(ref count);
            if (i == operationsPerInvoke)
            {
                wait.Set();
            }

            return Task.CompletedTask;
        }

        using IChannel<int> queue = new QueueChannel<int>();
        using Fiber fiber1 = new();
        using Fiber fiber2 = new();
        using Fiber fiber3 = new();
        using IDisposable sub = queue.Subscribe(fiber1, AsyncHandler);
        using IDisposable sub2 = queue.Subscribe(fiber2, AsyncHandler);
        using IDisposable sub3 = queue.Subscribe(fiber3, AsyncHandler);

        for (int j = 0; j < operationsPerInvoke; j++)
        {
            queue.Publish(j);
        }

        Assert.IsTrue(wait.WaitOne(15000, false));
    }
}
