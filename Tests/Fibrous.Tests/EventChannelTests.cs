using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class EventChannelTests
{
    [Test]
    public void BasicTest()
    {
        IEventChannel eventChannel = new EventChannel();
        using AutoResetEvent reset = new(false);

        async Task Receive()
        {
            reset.Set();
        }

        using Fiber fiber = new();

        eventChannel.Subscribe(fiber, Receive);

        eventChannel.Trigger();

        Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public async Task ThrottledTest()
    {
        IEventChannel eventChannel = new EventChannel();
        using AutoResetEvent reset = new(false);
        int i = 0;

        void Receive()
        {
            i++;
            if (i == 2)
            {
                reset.Set();
            }
        }

        using Fiber fiber = new();

        eventChannel.SubscribeThrottled(fiber, Receive, TimeSpan.FromSeconds(.5));
        for (int j = 0; j < 10; j++)
        {
            eventChannel.Trigger();
        }

        await Task.Delay(TimeSpan.FromSeconds(.6));
        for (int j = 0; j < 10; j++)
        {
            eventChannel.Trigger();
        }

        Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(2)));
        Assert.AreEqual(2, i);
    }
}
