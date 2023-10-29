using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class BoundedProductionTests
{
    [Test]
    public void SlowerConsumer()
    {
        using Fiber fiber1 = new(size: 4);
        using Fiber fiber2 = new();
        int count = 0;
        AutoResetEvent reset = new(false);

        void Action(int o)
        {
            count++;
            Thread.Sleep(100);
            if (count == 10)
            {
                reset.Set();
            }
        }


        Channel<int> channel = new();
        channel.Subscribe(fiber1, Action);
        fiber2.Schedule(() => channel.Publish(0), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20));
        Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void AsyncSlowerConsumer()
    {
        using AsyncFiber fiber1 = new(size: 4);
        using Fiber fiber2 = new();
        int count = 0;
        AutoResetEvent reset = new(false);

        async Task Action(int o)
        {
            count++;
            await Task.Delay(100);
            if (count == 10)
            {
                reset.Set();
            }
        }


        Channel<int> channel = new();
        channel.Subscribe(fiber1, Action);

        fiber2.Schedule(() => channel.Publish(0), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20));

        Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(4)));
    }
}
