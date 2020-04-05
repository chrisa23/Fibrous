using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class ReqReplyTests
    {
        [Test]
        public async Task BasicAsyncRequestReply()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            var fiber1 = new AsyncFiber();
            channel.SetRequestHandler(fiber1, request =>
            {
                request.Reply(request.Request + 1);
                return Task.CompletedTask;
            });
            using (var perfTimer = new PerfTimer(1000000))
            {
                for (var i = 0; i < 1000000; i++)
                {
                    var reply = await channel.SendRequest(0);
                   // Assert.AreEqual(1, reply);
                }
            }
        }

        [Test]
        public async Task BasicRequestReply()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            var fiber1 = new Fiber();
            channel.SetRequestHandler(fiber1, request => request.Reply(request.Request + 1));
            using (var perfTimer = new PerfTimer(1000000))
            {
                for (var i = 0; i < 1000000; i++)
                {
                    var reply = await channel.SendRequest(0);
                   // Assert.AreEqual(1, reply);
                }
            }
        }

        [Test]
        public async Task TimeOutRequestReply()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            var fiber1 = new AsyncFiber();

            async Task Reply(IRequest<int, int> request)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                request.Reply(request.Request + 1);
            };
            channel.SetRequestHandler(fiber1, Reply);

            var reply = await channel.SendRequest(0, TimeSpan.FromSeconds(1));
            Assert.IsFalse(reply.Succeeded);
        }

        [Test]
        public async Task TimeOutRequestReplySuccess()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            var fiber1 = new AsyncFiber();

            async Task Reply(IRequest<int, int> request)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                request.Reply(request.Request + 1);
            };
            channel.SetRequestHandler(fiber1, Reply);

            var reply = await channel.SendRequest(0, TimeSpan.FromSeconds(2));
            Assert.IsTrue(reply.Succeeded);
            Assert.AreEqual(1, reply.Value);
        }

        [Test]
        public async Task TimeOutRequestReplyCancel()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            
            //NOTE: either use an Exception Handling Executor or wrap methods using
            //the request's cancel token in a try catch
            var fiber1 = new AsyncFiber();

            async Task Reply(IRequest<int, int> request)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), request.CancellationToken);
                    request.Reply(request.Request + 1);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Canceled");
                }
            }
            
            channel.SetRequestHandler(fiber1, Reply);

            var reply = await channel.SendRequest(0, TimeSpan.FromSeconds(1));
            
            Assert.IsFalse(reply.Succeeded);

            var reply2 = await channel.SendRequest(1, TimeSpan.FromSeconds(5));
            Assert.IsTrue(reply2.Succeeded);
            Assert.AreEqual(2, reply2.Value);

            await Task.Delay(TimeSpan.FromSeconds(3));
        }

    }
}