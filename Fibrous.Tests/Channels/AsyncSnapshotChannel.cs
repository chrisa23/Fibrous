namespace Fibrous.Tests.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    internal class AsyncSnapshotChannel
    {
        [Test]
        public void AsyncSnapshot()
        {
            using (IFiber fiber = PoolFiber.StartNew())
            using (IFiber fiber2 = PoolFiber.StartNew())
            {
                var list = new List<string> { "Prime" };
                var channel = new SnapshotChannel<string>();
                channel.ReplyToPrimingRequest(fiber2, list.ToArray);
                var primeResult = new List<string>();
                string lastUpdate = "";
                Action<IEnumerable<string>> primed = primeResult.AddRange;
                Action<string> update = x => lastUpdate = x;
                channel.PrimedSubscribe(fiber, update, primed);
                Thread.Sleep(100);
                channel.Publish("hello");
                channel.Publish("hello2");
                Thread.Sleep(100);
                Assert.AreEqual("Prime", primeResult[0]);
                Assert.AreEqual("hello2", lastUpdate);
            }
        }
    }
}