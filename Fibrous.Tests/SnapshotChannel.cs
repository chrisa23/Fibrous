namespace Fibrous.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    internal class SnapshotChannel
    {
        [Test]
        public void Snapshot()
        {
            using (IFiber fiber = PoolFiber.StartNew())
            using (IFiber fiber2 = PoolFiber.StartNew())
            {
                var list = new List<string> { "Prime" };
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
                Assert.AreEqual("hello2", primeResult[primeResult.Count - 1]);
            }
        }
    }
}