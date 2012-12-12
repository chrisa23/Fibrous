namespace Fibrous.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class QueueChannelTests
    {
        [Test]
        public void Multiple()
        {
            var queues = new List<Fiber>();
            int receiveCount = 0;
            using (var reset = new AutoResetEvent(false))
            {
                var channel = new QueueChannel<int>();
                const int MessageCount = 100;
                var updateLock = new object();
                for (int i = 0; i < 5; i++)
                {
                    Action<int> onReceive = delegate
                    {
                        Thread.Sleep(15);
                        lock (updateLock)
                        {
                            receiveCount++;
                            if (receiveCount == MessageCount)
                                reset.Set();
                        }
                    };
                    Fiber fiber = PoolFiber.StartNew();
                    queues.Add(fiber);
                    channel.Subscribe(fiber, onReceive);
                }
                for (int i = 0; i < MessageCount; i++)
                    channel.Publish(i);
                Assert.IsTrue(reset.WaitOne(10000, false));
                queues.ForEach(q => q.Dispose());
            }
        }

        [Test]
        public void SingleConsumer()
        {
            int oneConsumed = 0;
            using (Fiber one = PoolFiber.StartNew())
            using (var reset = new AutoResetEvent(false))
            {
                var channel = new QueueChannel<int>();
                Action<int> onMsg = obj =>
                {
                    oneConsumed++;
                    if (oneConsumed == 20)
                        reset.Set();
                };
                channel.Subscribe(one, onMsg);
                for (int i = 0; i < 20; i++)
                    channel.Publish(i);
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }

        [Test]
        public void SingleConsumerWithException()
        {
            var failed = new List<Exception>();
            var exec = new ExceptionHandlingExecutor(failed.Add);
            using (Fiber one = PoolFiber.StartNew(exec))
            using (var reset = new AutoResetEvent(false))
            {
                var channel = new QueueChannel<int>();
                Action<int> onMsg = num =>
                {
                    if (num == 0)
                        throw new Exception();
                    reset.Set();
                };
                channel.Subscribe(one, onMsg);
                channel.Publish(0);
                channel.Publish(1);
                Assert.IsTrue(reset.WaitOne(10000, false));
                Assert.AreEqual(1, failed.Count);
            }
        }
    }
}