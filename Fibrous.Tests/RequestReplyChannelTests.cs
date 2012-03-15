using System;
using Fibrous.Channels;
using Fibrous.Fibers;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class RequestReplyChannelTests
    {
        [Test]
        public void SynchronousRequestReply()
        {
            using (var responder = new PoolFiber())
            {
                responder.Start();
                DateTime now = DateTime.Now;

                var timeCheck = new RequestReplyChannel<string, DateTime>();
                timeCheck.SetRequestHandler(responder, req => req.Publish(now));

                DateTime result = timeCheck.SendRequest("hello", TimeSpan.FromMilliseconds(10000));
                Assert.AreEqual(result, now);
            }
        }
    }
}