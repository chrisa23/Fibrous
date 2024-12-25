using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

public static class FiberTester
{
    public static void TestPubSubSimple(IFiber fiber)
    {
        using (fiber)
        using (AutoResetEvent reset = new(false))
        {
            Channel<string> channel = new();
            channel.Subscribe(fiber, obj =>
            {
                reset.Set();
                return Task.CompletedTask;
            });
            channel.Publish("hello");
            Assert.IsTrue(reset.WaitOne(5000, false));
        }
    }


    public static void TestPubSubWithFilter(IFiber fiber)
    {
        using (fiber)
        using (AutoResetEvent reset = new(false))
        {
            Channel<int> channel = new();

            Task OnMsg(int x)
            {
                Assert.IsTrue(x % 2 == 0);
                if (x == 4)
                {
                    reset.Set();
                }

                return Task.CompletedTask;
            }

            channel.Subscribe(fiber, OnMsg, x => x % 2 == 0);
            channel.Publish(1);
            channel.Publish(2);
            channel.Publish(3);
            channel.Publish(4);
            Assert.IsTrue(reset.WaitOne(5000, false));
        }
    }

    public static async Task TestReqReplyAsync(IFiber fiber)
    {
        RequestChannel<string, string> channel = new();
        using (fiber)
        using (channel.SetRequestHandler(fiber, async req => req.Reply("bye")))
        {
            string reply = await channel.SendRequestAsync("hello");
            Assert.AreEqual("bye", reply);
        }
    }

    public static void TestScheduling1(IFiber fiber)
    {
    }



    public static void TestBatching(IFiber fiber)
    {
        using (fiber)
        using (ManualResetEvent reset = new(false))
        {
            Channel<int> counter = new();
            int total = 0;

            Task Cb(IList<int> batch)
            {
                total += batch.Count;
                if (total == 10)
                {
                    reset.Set();
                }

                return Task.CompletedTask;
            }

            counter.SubscribeToBatch(fiber, Cb, TimeSpan.FromMilliseconds(1));
            for (int i = 0; i < 10; i++)
            {
                counter.Publish(i);
            }

            Assert.IsTrue(reset.WaitOne(10000, false));
        }
    }

    public static void TestBatchingWithKey(IFiber fiber)
    {
        using (fiber)
        using (ManualResetEvent reset = new(false))
        {
            Channel<int> counter = new();

            Task Cb(IDictionary<string, int> batch)
            {
                if (batch.ContainsKey("9"))
                {
                    reset.Set();
                }

                return Task.CompletedTask;
            }

            string KeyResolver(int x)
            {
                return x.ToString();
            }

            //disposed with fiber
            counter.SubscribeToKeyedBatch(fiber, KeyResolver, Cb, TimeSpan.FromMilliseconds(1));
            for (int i = 0; i < 10; i++)
            {
                counter.Publish(i);
            }

            Assert.IsTrue(reset.WaitOne(10000, false));
        }
    }


    public static void InOrderExecution(IFiber fiber)
    {
        using (fiber)
        using (AutoResetEvent reset = new(false))
        {
            Channel<int> channel = new();
            List<int> result = new();

            Task Command(int i)
            {
                result.Add(i);
                if (i == 99)
                {
                    reset.Set();
                }

                return Task.CompletedTask;
            }

            channel.Subscribe(fiber, Command);
            for (int i = 0; i < 100; i++)
            {
                channel.Publish(i);
            }

            Assert.IsTrue(reset.WaitOne(10000, false));
            Assert.AreEqual(100, result.Count);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
    }

    public static void TestPubSubWExtraFiber(IFiber fiber, IFiber fiber2)
    {
        using (fiber)
        using (fiber2)
        using (AutoResetEvent reset = new(false))
        using (AutoResetEvent reset2 = new(false))
        {
            Channel<string> channel = new();
            channel.Subscribe(fiber, obj =>
            {
                reset.Set();
                return Task.CompletedTask;
            });
            channel.Subscribe(fiber2, async obj => reset2.Set());
            channel.Publish("hello");
            Assert.IsTrue(reset.WaitOne(5000, false));
            Assert.IsTrue(reset2.WaitOne(5000, false));
        }
    }
}
