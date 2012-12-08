namespace Fibrous.Tests.Channels
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class RequestChannelTests
    {
        [Test]
        public void RequestChannelTest()
        {
            using (Fiber fiber = PoolFiber.StartNew())
            {
                var channel = new RequestChannel<string, string>();
                using (channel.SetRequestHandler(fiber, req => req.Reply("bye")))
                {
                    string reply = channel.SendRequest("hello").Receive(TimeSpan.FromSeconds(1)).Value;
                    Assert.AreEqual("bye", reply);
                }
            }
        }

        [Test]
        public void SynchronousRequestReply()
        {
            using (Fiber responder = PoolFiber.StartNew())
            {
                DateTime now = DateTime.Now;
                var timeCheck = new RequestChannel<string, DateTime>();
                using (timeCheck.SetRequestHandler(responder, req => req.Reply(now)))
                {
                    DateTime result = timeCheck.SendRequest("hello").Receive(TimeSpan.FromMilliseconds(10000)).Value;
                    Assert.AreEqual(result, now);
                }
            }
        }
    }
}