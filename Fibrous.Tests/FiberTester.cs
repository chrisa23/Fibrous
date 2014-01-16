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
                Action<int> onMsg = x =>
                {
                    Assert.IsTrue(x % 2 == 0);
                    if (x == 4)
                        reset.Set();
                };
                channel.Subscribe(fiber, onMsg, x => x % 2 == 0);
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
                string reply = channel.SendRequest("hello").Receive(TimeSpan.FromSeconds(1)).Value;
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
                Action<IList<int>> cb = batch =>
                {
                    total += batch.Count;
                    if (total == 10)
                        reset.Set();
                };
                counter.SubscribeToBatch(fiber, cb, TimeSpan.FromMilliseconds(1));
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
                Action<IDictionary<String, int>> cb = batch =>
                {
                    if (batch.ContainsKey("9"))
                        reset.Set();
                };
                Converter<int, String> keyResolver = x => x.ToString();
                //disposed with fiber
                counter.SubscribeToKeyedBatch(fiber, keyResolver, cb, TimeSpan.FromMilliseconds(1));
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
                Action command = () =>
                {
                    result.Add(count++);
                    if (count == 100)
                        reset.Set();
                };
                for (int i = 0; i < 100; i++)
                    fiber.Enqueue(command);
                Assert.IsTrue(reset.WaitOne(10000, false));
                Assert.AreEqual(100, count);
            }
        }
    }
}