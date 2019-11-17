using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class ReqReplyTests
    {
        [Test]
        public async Task BasicRequestReply()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            var fiber1 = PoolFiber.StartNew();
            channel.SetRequestHandler(fiber1, request => request.Reply(request.Request + 1));
            using (var perfTimer = new PerfTimer(1000000))
            {
                for (var i = 0; i < 1000000; i++)
                {
                    int reply = await channel.SendRequest(0);
                    Assert.AreEqual(1, reply);
                }
            }
        }
        [Test]
        public async Task BasicAsyncRequestReply()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            var fiber1 = AsyncFiber.StartNew();
            channel.SetRequestHandler(fiber1, request =>
            {
                request.Reply(request.Request + 1);
                return Task.CompletedTask;
            });
            using (var perfTimer = new PerfTimer(1000000))
            {
                for (var i = 0; i < 1000000; i++)
                {
                    int reply = await channel.SendRequest(0);
                    Assert.AreEqual(1, reply);
                }
            }
        }
    }
}