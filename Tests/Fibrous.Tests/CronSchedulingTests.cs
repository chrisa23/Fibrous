using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class CronSchedulingTests
{
    [Test]
    public void BasicAsyncTest()
    {
        int count = 0;
        using ManualResetEventSlim receivedTwo = new();

        Task Action()
        {
            if (Interlocked.Increment(ref count) == 2)
            {
                receivedTwo.Set();
            }

            return Task.CompletedTask;
        }

        using Fiber fiber = new();
        using (fiber.CronSchedule(Action, "0/1 * * * * ? *"))
        {
            TestWait.For(receivedTwo, TimeSpan.FromSeconds(5));
        }

        int countAfterDispose = Volatile.Read(ref count);

        Thread.Sleep(TimeSpan.FromSeconds(2));
        Assert.That(Volatile.Read(ref count), Is.EqualTo(countAfterDispose));
    }
}
