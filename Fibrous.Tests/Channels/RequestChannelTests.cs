using System;
using Fibrous.Channels;
using Fibrous.Fibers;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class RequestChannelTests
    {
        [Test]
        public void RequestChannelTest()
        {
            using (IFiber fiber = PoolFiber.StartNew())
            {
                var channel = new RequestChannel<string, string>();
                using (channel.SetRequestHandler(fiber, req => req.Reply("bye")))
                {
                    string reply = channel.SendRequest("hello", TimeSpan.FromSeconds(1));
                    Assert.AreEqual("bye", reply);
                }
            }
        }

        [Test]
        public void SynchronousRequestReply()
        {
            using (IFiber responder = PoolFiber.StartNew())
            {
                DateTime now = DateTime.Now;
                var timeCheck = new RequestChannel<string, DateTime>();
                using (timeCheck.SetRequestHandler(responder, req => req.Reply(now)))
                {
                    DateTime result = timeCheck.SendRequest("hello", TimeSpan.FromMilliseconds(10000));
                    Assert.AreEqual(result, now);
                }
            }
        }
    }
}