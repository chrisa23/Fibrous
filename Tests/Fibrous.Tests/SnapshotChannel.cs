using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
internal class SnapshotChannel
{
    [Test]
    public void AsyncSnapshot()
    {
        using Fiber fiber = new();
        using Fiber fiber2 = new();
        using AutoResetEvent snapshotReceived = new(false);
        using AutoResetEvent updatesReceived = new(false);
        List<string> list = new() {"Prime"};
        SnapshotChannel<string, string[]> channel = new();
        int updateCount = 0;

        Task<string[]> Reply()
        {
            return Task.FromResult(list.ToArray());
        }

        channel.ReplyToPrimingRequest(fiber2, Reply);
        List<string> primeResult = new();

        Task Update(string x)
        {
            primeResult.Add(x);
            if (Interlocked.Increment(ref updateCount) == 2)
            {
                updatesReceived.Set();
            }

            return Task.CompletedTask;
        }

        Task Snap(string[] x)
        {
            primeResult.AddRange(x);
            snapshotReceived.Set();
            return Task.CompletedTask;
        }

        channel.Subscribe(fiber, Update, Snap);
        TestWait.For(snapshotReceived);

        channel.Publish("hello");
        channel.Publish("hello2");
        TestWait.For(updatesReceived);

        Assert.AreEqual("Prime", primeResult[0]);
        Assert.AreEqual("hello2", primeResult[^1]);
    }
}
