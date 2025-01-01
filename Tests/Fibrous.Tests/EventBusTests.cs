using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class EventBusTests
{
    [Test]
    public void EventBusInt()
    {
        using Fiber fiber = new();
        using AutoResetEvent reset = new(false);
        EventBus<int>.Subscribe(fiber, _ =>
        {
             reset.Set();
             return Task.CompletedTask;
        });
        EventBus<int>.Publish(0);
        Assert.IsTrue(reset.WaitOne(100));
    }

    [Test]
    public void EventBusMixed()
    {
        using Fiber fiber = new();
        using Fiber fiber2 = new();
        using AutoResetEvent reset = new(false);
        using AutoResetEvent reset2 = new(false);
        EventBus<int>.Subscribe(fiber, _ =>
        {
            reset.Set();
            return Task.CompletedTask;
        });
        EventBus<string>.Subscribe(fiber, _ =>
        {
            reset2.Set();
            return Task.CompletedTask;
        });
        EventBus<int>.Publish(0);
        EventBus<string>.Publish("!");
        Assert.IsTrue(WaitHandle.WaitAll(new[] {reset, reset2}, 100));
    }
}
