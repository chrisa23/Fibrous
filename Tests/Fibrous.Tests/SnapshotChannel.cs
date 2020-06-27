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
            using var fiber = new Fiber();
            using var fiber2 = new Fiber();
            var list = new List<string> {"Prime"};
            var channel = new SnapshotChannel<string, string[]>();
            channel.ReplyToPrimingRequest(fiber2, list.ToArray);
            
            var primeResult = new List<string>();
            Action<string> update = primeResult.Add;
            Action<string[]> snap = primeResult.AddRange;
            
            channel.Subscribe(fiber, update, snap);
            
            Thread.Sleep(100);
            
            channel.Publish("hello");
            channel.Publish("hello2");
            
            Thread.Sleep(100);
            
            Assert.AreEqual("Prime", primeResult[0]);
            Assert.AreEqual("hello2", primeResult[^1]);
        }

        [Test]
        public void AsyncSnapshot()
        {
            using var fiber = new AsyncFiber();
            using var fiber2 = new AsyncFiber();
            var list = new List<string> { "Prime" };
            var channel = new SnapshotChannel<string, string[]>();

            Task<string[]> Reply() => Task.FromResult(list.ToArray());

            channel.ReplyToPrimingRequest(fiber2, Reply);
            var primeResult = new List<string>();

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

            Thread.Sleep(100);
            
            channel.Publish("hello");
            channel.Publish("hello2");
            
            Thread.Sleep(100);
            
            Assert.AreEqual("Prime", primeResult[0]);
            Assert.AreEqual("hello2", primeResult[^1]);
        }
    }
}