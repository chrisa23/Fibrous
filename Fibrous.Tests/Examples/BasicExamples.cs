using System;
using System.Collections.Generic;
using System.Threading;
using Fibrous.Channels;
using Fibrous.Fibers;
using Fibrous.Fibers.Thread;
using Fibrous.Fibers.ThreadPool;
using Fibrous.Scheduling;
using NUnit.Framework;

namespace Fibrous.Tests.Examples
{
    [TestFixture]
    public class BasicExamples
    {
        [Test]
        public void PubSubWithPool()
        {
            //PoolFiber uses the .NET thread pool by default
            using (var fiber = new PoolFiber())
            {
                fiber.Start();
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
            using (var fiber = new ThreadFiber())
            {
                fiber.Start();
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
                        Assert.IsTrue(x%2 == 0);
                        if (x == 4)
                        {
                            reset.Set();
                        }
                    };
                //     var sub = new ChannelSubscription<int>(fiber, onMsg);
                //sub.FilterOnProducerThread = ;
                channel.Subscribe(fiber, onMsg, x => x%2 == 0);
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

                counter.SubscribeToBatch(fiber, cb, TimeSpan.FromMilliseconds(1));

                for (int i = 0; i < 10; i++)
                {
                    counter.Publish(i);
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
                counter.SubscribeToKeyedBatch(fiber, keyResolver, cb, TimeSpan.FromMilliseconds(1));

                for (int i = 0; i < 10; i++)
                {
                    counter.Publish(i);
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
                channel.SetRequestHandler(fiber, req => req.Publish("bye"));
                string reply = channel.SendRequest("hello", TimeSpan.FromSeconds(1));
                Assert.AreEqual("bye", reply);
            }
        }

        //[Test]
        //public void ShouldIncreasePoolFiberSubscriberCountByOne()
        //{
        //    var fiber = new PoolFiber();
        //    fiber.Start();
        //    var channel = new Channel<int>();

        //    Assert.AreEqual(0, fiber.NumSubscriptions);
        //    Assert.AreEqual(0, channel.NumSubscribers);
        //    channel.Subscribe(fiber, x => { });

        //    Assert.AreEqual(1, fiber.NumSubscriptions);
        //    Assert.AreEqual(1, channel.NumSubscribers);
        //    fiber.Dispose();

        //    Assert.AreEqual(0, fiber.NumSubscriptions);
        //    Assert.AreEqual(0, channel.NumSubscribers);
        //}

        //[Test]
        //public void ShouldIncreaseThreadFiberSubscriberCountByOne()
        //{
        //    var fiber = new ThreadFiber();
        //    fiber.Start();
        //    var channel = new Channel<int>();

        //    Assert.AreEqual(0, fiber.NumSubscriptions);
        //    Assert.AreEqual(0, channel.NumSubscribers);
        //    channel.Subscribe(fiber, x => { });

        //    Assert.AreEqual(1, fiber.NumSubscriptions);
        //    Assert.AreEqual(1, channel.NumSubscribers);
        //    fiber.Dispose();

        //    Assert.AreEqual(0, fiber.NumSubscriptions);
        //    Assert.AreEqual(0, channel.NumSubscribers);
        //}

        //[Test]
        //public void ShouldIncreaseStubFiberSubscriberCountByOne()
        //{
        //    var fiber = new TestFiber();
        //    fiber.Start();
        //    var channel = new Channel<int>();

        //    Assert.AreEqual(0, fiber.NumSubscriptions);
        //    Assert.AreEqual(0, channel.NumSubscribers);
        //    channel.Subscribe(fiber, x => { });

        //    Assert.AreEqual(1, fiber.NumSubscriptions);
        //    Assert.AreEqual(1, channel.NumSubscribers);
        //    fiber.Dispose();

        //    Assert.AreEqual(0, fiber.NumSubscriptions);
        //    Assert.AreEqual(0, channel.NumSubscribers);
        //}

        //[Test]
        //public void UnsubscriptionShouldRemoveSubscriber()
        //{
        //    var fiber = new PoolFiber();
        //    fiber.Start();
        //    var channel = new Channel<int>();

        //    Assert.AreEqual(0, fiber.NumSubscriptions);
        //    Assert.AreEqual(0, channel.NumSubscribers);
        //    IDisposable unsubscriber = channel.Subscribe(fiber, x => { });

        //    Assert.AreEqual(1, fiber.NumSubscriptions);
        //    Assert.AreEqual(1, channel.NumSubscribers);
        //    unsubscriber.Dispose();

        //    Assert.AreEqual(0, fiber.NumSubscriptions);
        //    Assert.AreEqual(0, channel.NumSubscribers);
        //}

        //[Test]
        //public void Snapshot()
        //{
        //    using (var fiber = new PoolFiber())
        //    using (var fiber2 = new PoolFiber())
        //    {
        //        fiber.Start();
        //        fiber2.Start();

        //        var channel = new SnapshotChannel<string, string>(TimeSpan.FromSeconds(1));
        //        channel.ReplyToPrimingRequest(fiber2, () => "Prime");
        //        string primeResult = "";
        //        string lastUpdate = "";
        //        Action<string> primed = x => primeResult = x;
        //        Action<string> update = x => lastUpdate = x;
        //        channel.PrimedSubscribe(fiber, update, primed);
        //        channel.Publish("hello");
        //        channel.Publish("hello2");

        //        Thread.Sleep(100);

        //        Assert.AreEqual("Prime", primeResult);
        //        Assert.AreEqual("hello2", lastUpdate);
        //    }
        //}

        //[Test]
        //public void AsyncSnapshot()
        //{
        //    using (var fiber = new PoolFiber())
        //    using (var fiber2 = new PoolFiber())
        //    {
        //        fiber.Start();
        //        fiber2.Start();
        //        var list = new List<string>();

        //        var channel = new AsyncSnapshotChannel<string, IEnumerable<string>>();
        //        channel.ReplyToPrimingRequest(fiber2, () => list);
        //        IEnumerable<string> primeResult;
        //        string lastUpdate = "";
        //        Action<IEnumerable<string>> primed = x => primeResult = x;
        //        Action<string> update = x => lastUpdate = x;
        //        channel.PrimedSubscribe(fiber, update, primed);

        //        Thread.Sleep(100);
        //              channel.Publish("hello");
        //        channel.Publish("hello2");
        //       // Assert.AreEqual("Prime", primeResult);
        //        Assert.AreEqual("hello2", lastUpdate);
        //    }


        //}

        //private class AsyncSnapshotUtil
        //{
        //    public string HandleRequest()
        //}
    }
}