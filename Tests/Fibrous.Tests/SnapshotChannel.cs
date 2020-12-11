using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    internal class SnapshotChannel
    {
        [Test]
        public void Snapshot()
        {
            using Fiber fiber = new Fiber();
            using Fiber fiber2 = new Fiber();
            List<string> list = new List<string> {"Prime"};
            SnapshotChannel<string, string[]> channel = new SnapshotChannel<string, string[]>();
            channel.ReplyToPrimingRequest(fiber2, list.ToArray);

            List<string> primeResult = new List<string>();
            Action<string> update = primeResult.Add;
            Action<string[]> snap = primeResult.AddRange;

            channel.Subscribe(fiber, update, snap);

            Thread.Sleep(500);

            channel.Publish("hello");
            channel.Publish("hello2");

            Thread.Sleep(500);

            Assert.AreEqual("Prime", primeResult[0]);
            Assert.AreEqual("hello2", primeResult[^1]);
        }

        [Test]
        public void AsyncSnapshot()
        {
            using AsyncFiber fiber = new AsyncFiber();
            using AsyncFiber fiber2 = new AsyncFiber();
            List<string> list = new List<string> {"Prime"};
            SnapshotChannel<string, string[]> channel = new SnapshotChannel<string, string[]>();

            Task<string[]> Reply()
            {
                return Task.FromResult(list.ToArray());
            }

            channel.ReplyToPrimingRequest(fiber2, Reply);
            List<string> primeResult = new List<string>();

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
}
