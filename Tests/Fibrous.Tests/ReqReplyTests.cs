using System;
using System.Threading;
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
        Fiber fiber = new();
        channel.SetRequestHandler(fiber, request =>
        {
            request.Reply(request.Request + 1);
            return Task.CompletedTask;
        });

        using PerfTimer perfTimer = new(1000000);
        for (int i = 0; i < 1000000; i++)
        {
            int reply = await channel.SendRequestAsync(0);
            _ = reply;
        }
    }

    [Test]
    public async Task TimeOutRequestReplyAsync()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber = new();

        static async Task Reply(IRequest<int, int> request)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            request.Reply(request.Request + 1);
        }

        channel.SetRequestHandler(fiber, Reply);

        Reply<int> reply = await channel.SendRequestAsync(0, TimeSpan.FromSeconds(1));
        Assert.IsFalse(reply.Succeeded);
    }

    [Test]
    public async Task TimeOutRequestReplySuccessAsync()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber = new();

        static async Task Reply(IRequest<int, int> request)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            request.Reply(request.Request + 1);
        }

        channel.SetRequestHandler(fiber, Reply);

        Reply<int> reply = await channel.SendRequestAsync(0, TimeSpan.FromSeconds(2));
        Assert.IsTrue(reply.Succeeded);
        Assert.AreEqual(1, reply.Value);
    }

    [Test]
    public async Task TimeOutRequestReplyCancel()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber = new();

        static async Task Reply(IRequest<int, int> request)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), request.CancellationToken);
                request.Reply(request.Request + 1);
            }
            catch (TaskCanceledException)
            {
            }
        }

        channel.SetRequestHandler(fiber, Reply);

        Reply<int> reply = await channel.SendRequestAsync(0, TimeSpan.FromSeconds(1));
        Assert.IsFalse(reply.Succeeded);

        Reply<int> reply2 = await channel.SendRequestAsync(1, TimeSpan.FromSeconds(5));
        Assert.IsTrue(reply2.Succeeded);
        Assert.AreEqual(2, reply2.Value);

        await Task.Delay(TimeSpan.FromSeconds(3));
    }

    [Test]
    public async Task LateReplyAfterTimeout_IsIgnored()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber = new(error => Assert.Fail(error.ToString()));

        static async Task Reply(IRequest<int, int> request)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            request.Reply(request.Request + 1);
        }

        channel.SetRequestHandler(fiber, Reply);

        Reply<int> reply = await channel.SendRequestAsync(1, TimeSpan.FromMilliseconds(10));
        Assert.IsFalse(reply.Succeeded);

        await Task.Delay(TimeSpan.FromMilliseconds(150));
    }

    [Test]
    public async Task DuplicateReply_IsIgnored()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber = new(error => Assert.Fail(error.ToString()));

        channel.SetRequestHandler(fiber, request =>
        {
            request.Reply(request.Request + 1);
            request.Reply(request.Request + 2);
            return Task.CompletedTask;
        });

        int reply = await channel.SendRequestAsync(1);
        Assert.AreEqual(2, reply);
    }

    [Test]
    public async Task CancellationTokenRequest_Cancels()
    {
        IRequestChannel<int, int> channel = new RequestChannel<int, int>();
        Fiber fiber = new();

        channel.SetRequestHandler(fiber, async request =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1), request.CancellationToken);
            request.Reply(request.Request + 1);
        });

        using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(20));
        Reply<int> reply = await channel.SendRequestAsync(1, cts.Token);

        Assert.IsFalse(reply.Succeeded);
    }

    [Test]
    public async Task DisposedCallbackSubscription_IgnoresReply()
    {
        RequestChannel<int, int> channel = new();
        Fiber serverFiber = new(error => Assert.Fail(error.ToString()));
        Fiber clientFiber = new(error => Assert.Fail(error.ToString()));
        int replyCount = 0;
        using ManualResetEventSlim allowReply = new(false);

        channel.SetRequestHandler(serverFiber, async request =>
        {
            allowReply.Wait(TimeSpan.FromSeconds(1));
            request.Reply(request.Request + 1);
            await Task.CompletedTask;
        });

        IDisposable sub = channel.SendRequest(1, clientFiber, reply =>
        {
            replyCount++;
            return Task.CompletedTask;
        });

        sub.Dispose();
        allowReply.Set();

        await Task.Delay(TimeSpan.FromMilliseconds(150));

        Assert.AreEqual(0, replyCount);
    }
}
