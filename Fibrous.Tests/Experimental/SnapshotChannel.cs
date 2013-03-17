namespace Fibrous.Tests.Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Experimental;
    using NUnit.Framework;

    [TestFixture]
    internal class SnapshotChannel
    {
        [Test]
        public void Snapshot()
        {
            using (Fiber fiber = PoolFiber.StartNew())
            using (Fiber fiber2 = PoolFiber.StartNew())
            {
                var list = new List<string> { "Prime" };
                var channel = new SnapshotChannel<string>();
                channel.ReplyToPrimingRequest(fiber2, list.ToArray);
                var primeResult = new List<string>();
                Action<string> update = primeResult.Add;
                channel.Subscribe(fiber, update);
                Thread.Sleep(100);
                channel.Publish("hello");
                channel.Publish("hello2");
                Thread.Sleep(100);
                Assert.AreEqual("Prime", primeResult[0]);
                Assert.AreEqual("hello2", primeResult[primeResult.Count-1]);
            }
        }
    }
}