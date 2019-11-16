using System.Threading.Tasks;
using Fibrous.Channels;

namespace Fibrous.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NUnit.Framework;

    public static class FiberTester
    {
        public static void TestPubSubSimple(IFiber fiber)
        {
            using (fiber)
            using (var reset = new AutoResetEvent(false))
            {
                var channel = new Channel<string>();
                channel.Subscribe(fiber, obj => reset.Set());
                channel.Publish("hello");
                Assert.IsTrue(reset.WaitOne(5000, false));
            }
        }

        public static void TestPubSubWithFilter(IFiber fiber)
        {
            using (fiber)
            using (var reset = new AutoResetEvent(false))
            {
                var channel = new Channel<int>();

                void OnMsg(int x)
                {
                    Assert.IsTrue(x % 2 == 0);
                    if (x == 4)
                        reset.Set();
                }

                channel.Subscribe(fiber, OnMsg, x => x % 2 == 0);
                channel.Publish(1);
                channel.Publish(2);
                channel.Publish(3);
                channel.Publish(4);
                Assert.IsTrue(reset.WaitOne(5000, false));
            }
        }

        public static void TestReqReply1(IFiber fiber)
        {
            var channel = new RequestChannel<string, string>();
            using (fiber)
            using (channel.SetRequestHandler(fiber, req => req.Reply("bye")))
            {
                string reply = channel.SendRequest("hello").Result;
                Assert.AreEqual("bye", reply);
            }
        }

        public static void TestScheduling1(IFiber fiber)
        {
        }

        public static void TestBatching(IFiber fiber)
        {
            using (fiber)
            using (var reset = new ManualResetEvent(false))
            {
                var counter = new Channel<int>();
                int total = 0;

                void Cb(IList<int> batch)
                {
                    total += batch.Count;
                    if (total == 10)
                        reset.Set();
                }

                counter.SubscribeToBatch(fiber, Cb, TimeSpan.FromMilliseconds(1));
                for (int i = 0; i < 10; i++)
                    counter.Publish(i);
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }

        public static void TestBatchingWithKey(IFiber fiber)
        {
            using (fiber)
            using (var reset = new ManualResetEvent(false))
            {
                var counter = new Channel<int>();

                void Cb(IDictionary<string, int> batch)
                {
                    if (batch.ContainsKey("9"))
                        reset.Set();
                }

                string KeyResolver(int x) => x.ToString();
                //disposed with fiber
                counter.SubscribeToKeyedBatch(fiber, KeyResolver, Cb, TimeSpan.FromMilliseconds(1));
                for (int i = 0; i < 10; i++)
                    counter.Publish(i);
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }

        public static void ExecuteOnlyAfterStart(IFiber fiber)
        {
            using (fiber)
            using (var reset = new AutoResetEvent(false))
            {
                fiber.Enqueue(() => reset.Set());
                Assert.IsFalse(reset.WaitOne(1, false));
                fiber.Start();
                Assert.IsTrue(reset.WaitOne(1000, false));
            }
        }

        public static void InOrderExecution(IFiber fiber)
        {
            using (fiber)
            using (var reset = new AutoResetEvent(false))
            {
                int count = 0;
                var result = new List<int>();

                void Command()
                {
                    result.Add(count++);
                    if (count == 100)
                        reset.Set();
                }

                for (int i = 0; i < 100; i++)
                    fiber.Enqueue(Command);
                Assert.IsTrue(reset.WaitOne(10000, false));
                Assert.AreEqual(100, count);
            }
        }
        public static void InOrderExecution(IAsyncFiber fiber)
        {
            using (fiber)
            using (var reset = new AutoResetEvent(false))
            {
                int count = 0;
                var result = new List<int>();

                Task Command()
                {
                    result.Add(count++);
                    if (count == 100)
                        reset.Set();
                    return Task.CompletedTask;
                }

                for (int i = 0; i < 100; i++)
                    fiber.Enqueue(Command);
                Assert.IsTrue(reset.WaitOne(10000, false));
                Assert.AreEqual(100, count);
            }
        }

        public static void TestPubSubWExtraFiber(IFiber fiber, IFiber fiber2)
        {
            using (fiber)
            using (fiber2)
            using (var reset = new AutoResetEvent(false))
            using (var reset2 = new AutoResetEvent(false))
            {
                var channel = new Channel<string>();
                channel.Subscribe(fiber, obj => reset.Set());
                channel.Subscribe(fiber2, obj => reset2.Set());
                channel.Publish("hello");
                Assert.IsTrue(reset.WaitOne(5000, false));
                Assert.IsTrue(reset2.WaitOne(5000, false));
            }
        }

        public static void ExecuteOnlyAfterStart(AsyncFiber fiber)
        {
            using (fiber)
            using (var reset = new AutoResetEvent(false))
            {
                fiber.Enqueue( async () => reset.Set());
                Assert.IsFalse(reset.WaitOne(1, false));
                fiber.Start();
                Assert.IsTrue(reset.WaitOne(1000, false));
            }
        }
    }
}