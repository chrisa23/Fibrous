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
        List<string> list = new() {"Prime"};
        SnapshotChannel<string, string[]> channel = new();

        Task<string[]> Reply()
        {
            return Task.FromResult(list.ToArray());
        }

        channel.ReplyToPrimingRequest(fiber2, Reply);
        List<string> primeResult = new();

        Task Update(string x)
        {
            primeResult.Add(x);
            return Task.CompletedTask;
        }

        Task Snap(string[] x)
        {
            primeResult.AddRange(x);
            return Task.CompletedTask;
        }

        channel.Subscribe(fiber, Update, Snap);

        Thread.Sleep(500);

        channel.Publish("hello");
        channel.Publish("hello2");

        Thread.Sleep(500);

        Assert.AreEqual("Prime", primeResult[0]);
        Assert.AreEqual("hello2", primeResult[^1]);
    }
}
