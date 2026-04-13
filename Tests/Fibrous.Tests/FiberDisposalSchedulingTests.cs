using System;
using System.Threading;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class FiberDisposalSchedulingTests
{
    [Test]
    public void DisposingFiberPreventsScheduledCallbackFromExecuting()
    {
        int executed = 0;
        using AutoResetEvent reset = new(false);
        Fiber fiber = new();

        fiber.Schedule(() =>
        {
            Interlocked.Increment(ref executed);
            reset.Set();
        }, TimeSpan.FromMilliseconds(200));

        fiber.Dispose();

        Assert.IsFalse(reset.WaitOne(TimeSpan.FromMilliseconds(400)));
        Assert.AreEqual(0, executed);
    }
}
