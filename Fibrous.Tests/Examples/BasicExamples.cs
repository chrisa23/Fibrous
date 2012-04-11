namespace Fibrous.Tests.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    public class BasicExamples
    {
        [Test]
        public void PubSubWithPool()
        {
            //PoolFiber uses the .NET thread pool by default
            using (IFiber fiber = PoolFiber.StartNew())
            {
                var channel = new Channel<string>();
                var reset = new AutoResetEvent(false);
                channel.Subscribe(fiber, delegate { reset.Set(); });
                channel.Publish("hello");
                Assert.IsTrue(reset.WaitOne(5000, false));
            }
        }

        [Test]
        public void PubSubWithDedicatedThread()
        {
            using (IFiber fiber = ThreadFiber.StartNew())
            {
                var channel = new Channel<string>();
                var reset = new AutoResetEvent(false);
                channel.Subscribe(fiber, delegate { reset.Set(); });
                channel.Publish("hello");
                Assert.IsTrue(reset.WaitOne(5000, false));
            }
        }

        [Test]
        public void PubSubWithDedicatedThreadWithFilter()
        {
            using (var fiber = new ThreadFiber())
            {
                fiber.Start();
                var channel = new Channel<int>();
                var reset = new AutoResetEvent(false);
                Action<int> onMsg = x =>
                {
                    Assert.IsTrue(x % 2 == 0);
                    if (x == 4)
                    {
                        reset.Set();
                    }
                };
                //     var sub = new ChannelSubscription<int>(fiber, onMsg);
                //sub.FilterOnProducerThread = ;
                channel.Subscribe(fiber, onMsg, x => x % 2 == 0);
                channel.Publish(1);
                channel.Publish(2);
                channel.Publish(3);
                channel.Publish(4);
                Assert.IsTrue(reset.WaitOne(5000, false));
            }
        }

        [Test]
        public void Batching()
        {
            using (var fiber = new ThreadFiber())
            {
                fiber.Start();
                var counter = new Channel<int>();
                var reset = new ManualResetEvent(false);
                int total = 0;
                Action<IList<int>> cb = delegate(IList<int> batch)
                {
                    total += batch.Count;
                    if (total == 10)
                    {
                        reset.Set();
                    }
                };
                using (counter.SubscribeToBatch(fiber, cb, TimeSpan.FromMilliseconds(1)))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        counter.Publish(i);
                    }
                }
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }

        [Test]
        public void BatchingWithKey()
        {
            using (var fiber = new ThreadFiber())
            {
                fiber.Start();
                var counter = new Channel<int>();
                var reset = new ManualResetEvent(false);
                Action<IDictionary<String, int>> cb = delegate(IDictionary<String, int> batch)
                {
                    if (batch.ContainsKey("9"))
                    {
                        reset.Set();
                    }
                };
                Converter<int, String> keyResolver = x => x.ToString();
                using (counter.SubscribeToKeyedBatch(fiber, keyResolver, cb, TimeSpan.FromMilliseconds(1)))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        counter.Publish(i);
                    }
                }
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }

        [Test]
        public void RequestReply()
        {
            using (var fiber = new PoolFiber())
            {
                fiber.Start();
                var channel = new RequestReplyChannel<string, string>();
                using (channel.SetRequestHandler(fiber, req => req.Publish("bye")))
                {
                    string reply = channel.SendRequest("hello", TimeSpan.FromSeconds(1));
                    Assert.AreEqual("bye", reply);
                }
            }
        }

        [Test]
        public void AsyncSnapshot()
        {
            using (IFiber fiber = PoolFiber.StartNew())
            using (IFiber fiber2 = PoolFiber.StartNew())
            {
                var list = new List<string> { "Prime" };
                var channel = new AsyncSnapshotChannel<string, IEnumerable<string>>();
                channel.ReplyToPrimingRequest(fiber2, list.ToArray);
                var primeResult = new List<string>();
                string lastUpdate = "";
                Action<IEnumerable<string>> primed = primeResult.AddRange;
                Action<string> update = x => lastUpdate = x;
                channel.PrimedSubscribe(fiber, update, primed);
                Thread.Sleep(100);
                channel.Publish("hello");
                channel.Publish("hello2");
                Thread.Sleep(100);
                Assert.AreEqual("Prime", primeResult[0]);
                Assert.AreEqual("hello2", lastUpdate);
            }
        }
    }
}