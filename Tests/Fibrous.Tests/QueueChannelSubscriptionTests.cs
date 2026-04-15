using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class QueueChannelSubscriptionTests
{
    [Test]
    public void DisposedSubscriber_NoLongerReceivesMessages()
    {
        using AutoResetEvent reset = new(false);
        using Fiber activeFiber = new();
        using Fiber removedFiber = new();
        using QueueChannel<int> channel = new();

        int activeCount = 0;
        int removedCount = 0;

        IDisposable removedSub = channel.Subscribe(removedFiber, message =>
        {
            Interlocked.Increment(ref removedCount);
            return Task.CompletedTask;
        });

        channel.Subscribe(activeFiber, message =>
        {
            if (Interlocked.Increment(ref activeCount) == 20)
            {
                reset.Set();
            }

            return Task.CompletedTask;
        });

        removedSub.Dispose();

        for (int i = 0; i < 20; i++)
        {
            channel.Publish(i);
        }

        TestWait.For(reset, 10000);
        Assert.That(activeCount, Is.EqualTo(20));
        Assert.That(removedCount, Is.EqualTo(0));
    }
}
