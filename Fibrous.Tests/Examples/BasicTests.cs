namespace Fibrous.Tests.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class BasicTests
    {
        [Test]
        public void Batching()
        {
            using (Fiber fiber = ThreadFiber.StartNew())
            {
                var counter = new Channel<int>();
                var reset = new ManualResetEvent(false);
                int total = 0;
                Action<IList<int>> cb = delegate(IList<int> batch)
                {
                    total += batch.Count;
                    if (total == 10)
                        reset.Set();
                };
                using (counter.SubscribeToBatch(fiber, cb, TimeSpan.FromMilliseconds(1)))
                {
                    for (int i = 0; i < 10; i++)
                        counter.Publish(i);
                    Assert.IsTrue(reset.WaitOne(10000, false));
                }
            }
        }

        [Test]
        public void BatchingWithKey()
        {
            using (Fiber fiber = ThreadFiber.StartNew())
            {
                var counter = new Channel<int>();
                var reset = new ManualResetEvent(false);
                Action<IDictionary<String, int>> cb = delegate(IDictionary<String, int> batch)
                {
                    if (batch.ContainsKey("9"))
                        reset.Set();
                };
                Converter<int, String> keyResolver = x => x.ToString();
                using (counter.SubscribeToKeyedBatch(fiber, keyResolver, cb, TimeSpan.FromMilliseconds(1)))
                {
                    for (int i = 0; i < 10; i++)
                        counter.Publish(i);
                }
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }

        [Test]
        public void PubSubWithDedicatedThread()
        {
            using (Fiber fiber = ThreadFiber.StartNew())
            {
                var channel = new Channel<string>();
                using (var reset = new AutoResetEvent(false))
                {
                    channel.Subscribe(fiber, delegate { reset.Set(); });
                    channel.Publish("hello");
                    Assert.IsTrue(reset.WaitOne(5000, false));
                }
            }
        }

        [Test]
        public void PubSubWithDedicatedThreadWithFilter()
        {
            using (var reset = new AutoResetEvent(false))
            using (Fiber fiber = ThreadFiber.StartNew())
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
                //channel.Publish(5);
                //channel.Publish(6);
                Assert.IsTrue(reset.WaitOne(5000, false));
            }
        }

        [Test]
        public void PubSubWithPool()
        {
            using (Fiber fiber = PoolFiber.StartNew())
            {
                var channel = new Channel<string>();
                using (var reset = new AutoResetEvent(false))
                {
                    channel.Subscribe(fiber, delegate { reset.Set(); });
                    channel.Publish("hello");
                    Assert.IsTrue(reset.WaitOne(5000, false));
                }
            }
        }
    }
}