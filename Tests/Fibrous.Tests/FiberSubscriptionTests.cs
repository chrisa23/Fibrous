using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class FiberSubscriptionTests
{
    [Test]
    public async Task FilteredSubscribe()
    {
        List<int> result = new List<int>();
        using AsyncFiber fiber = new ();
        using Channel<int> channel = new Channel<int>();
        using IDisposable sub = channel.Subscribe(fiber, async x =>  result.Add(x), x => x > 10);
        for (int i = 0; i < 20; i++)
        {
            channel.Publish(i);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500));
        Assert.AreEqual(9, result.Count);
    }
}
