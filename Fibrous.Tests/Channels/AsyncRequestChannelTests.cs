namespace Fibrous.Tests.Channels
{
    using System;
    using System.Threading;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    public class AsyncRequestChannelTests
    {
        [Test]
        public void AsynchronousRequestReply()
        {
            var requester = new PoolFiber();
            requester.Start();
            var replier = new PoolFiber();
            replier.Start();
            var received = new AutoResetEvent(false);
            DateTime now = DateTime.Now;
            var timeCheck = new RequestChannel<string, DateTime>();
            timeCheck.SetRequestHandler(replier, req => req.Reply(now));
            DateTime result = DateTime.MinValue;
            timeCheck.SendRequest("hello",
                requester,
                x =>
                {
                    result = x;
                    received.Set();
                });
            received.WaitOne(1000, false);
            Assert.AreEqual(result, now);
        }

        [Test]
        public void AsynchronousRequestWithMultipleReplies()
        {
            var requester = new PoolFiber();
            requester.Start();
            var replier = new PoolFiber();
            replier.Start();
            var countChannel = new RequestChannel<string, int>();
            countChannel.SetRequestHandler(replier,
                req =>
                {
                    for (int i = 0; i < 5; i++)
                        req.Reply(i);
                });
            var received = new CountdownEvent(5);
            int result = -1;
            countChannel.SendRequest("hello",
                requester,
                x =>
                {
                    result = x;
                    received.Signal();
                });
            received.Wait(1000);
            Assert.AreEqual(4, result);
        }
    }
}