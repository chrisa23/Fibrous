using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class ReqReplyTests
{
    [Test]
    public async Task BasicAsyncRequestReplyAsync()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber1 = new();
        channel.SetRequestHandler(fiber1, request =>
        {
            request.Reply(request.Request + 1);
            return Task.CompletedTask;
        });
        using (PerfTimer perfTimer = new(1000000))
        {
            for (int i = 0; i < 1000000; i++)
            {
                int reply = await channel.SendRequestAsync(0);
                // Assert.AreEqual(1, reply);
            }
        }
    }

    /*[Test]
    public async Task BasicRequestReplyAsync()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber1 = new();
        channel.SetRequestHandler(fiber1, request => request.Reply(request.Request + 1));
        using (PerfTimer perfTimer = new(1000000))
        {
            for (int i = 0; i < 1000000; i++)
            {
                int reply = await channel.SendRequestAsync(0);
                // Assert.AreEqual(1, reply);
            }
        }
    }*/

    [Test]
    public async Task TimeOutRequestReplyAsync()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber1 = new();

        static async Task Reply(IRequest<int, int> request)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            request.Reply(request.Request + 1);
        }

        ;
        channel.SetRequestHandler(fiber1, Reply);

        Reply<int> reply = await channel.SendRequestAsync(0, TimeSpan.FromSeconds(1));
        Assert.IsFalse(reply.Succeeded);
    }

    [Test]
    public async Task TimeOutRequestReplySuccessAsync()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber1 = new();

        static async Task Reply(IRequest<int, int> request)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            request.Reply(request.Request + 1);
        }

        ;
        channel.SetRequestHandler(fiber1, Reply);

        Reply<int> reply = await channel.SendRequestAsync(0, TimeSpan.FromSeconds(2));
        Assert.IsTrue(reply.Succeeded);
        Assert.AreEqual(1, reply.Value);
    }

    [Test]
    public async Task TimeOutRequestReplyCancel()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();

        //NOTE: either use an Exception Handling Executor or wrap methods using
        //the request's cancel token in a try catch
        Fiber fiber1 = new();

        static async Task Reply(IRequest<int, int> request)
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

        Reply<int> reply = await channel.SendRequestAsync(0, TimeSpan.FromSeconds(1));

        Assert.IsFalse(reply.Succeeded);

        Reply<int> reply2 = await channel.SendRequestAsync(1, TimeSpan.FromSeconds(5));
        Assert.IsTrue(reply2.Succeeded);
        Assert.AreEqual(2, reply2.Value);

        await Task.Delay(TimeSpan.FromSeconds(3));
    }
}
