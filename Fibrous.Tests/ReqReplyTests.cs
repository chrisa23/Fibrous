using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class ReqReplyTests
    {
        [Test]
        public void TestName()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            var fiber1 = PoolFiber.StartNew();
            channel.SetRequestHandler(fiber1, request => request.Reply(request.Request + 1));
            using (var perfTimer = new PerfTimer(1000000))
            {
                for (var i = 0; i < 1000000; i++) channel.SendRequest(0).Wait();
            }
        }
    }
}