namespace Fibrous.Tests
{
    using System.Numerics;
    using NUnit.Framework;

    [TestFixture]
    public class ReqReplyTests
    {
        [Test]
        public void TestName()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            IFiber fiber1 = PoolFiber.StartNew();
            channel.SetRequestHandler(fiber1, request => request.Reply(request.Request + 1));
            using (var perfTimer = new PerfTimer(1000000))
            {
                for (int i = 0; i < 1000000; i++)
                {
                    channel.SendRequest(0).Wait();
                }
            }
        }
    }
}