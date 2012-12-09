namespace Fibrous.Tests.Channels
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
            var reset = new AutoResetEvent(false);
            var channel = new QueueChannel<int>();
            int messageCount = 100;
            var updateLock = new object();
            for (int i = 0; i < 5; i++)
            {
                Action<int> onReceive = delegate
                {
                    Thread.Sleep(15);
                    lock (updateLock)
                    {
                        receiveCount++;
                        if (receiveCount == messageCount)
                            reset.Set();
                    }
                };
                var fiber = new PoolFiber();
                fiber.Start();
                queues.Add(fiber);
                channel.Subscribe(fiber, onReceive);
            }
            for (int i = 0; i < messageCount; i++)
                channel.Publish(i);
            Assert.IsTrue(reset.WaitOne(10000, false));
            queues.ForEach(delegate(Fiber q) { q.Dispose(); });
        }

        [Test]
        public void SingleConsumer()
        {
            var one = new PoolFiber();
            one.Start();
            int oneConsumed = 0;
            var reset = new AutoResetEvent(false);
            using (one)
            {
                var channel = new QueueChannel<int>();
                Action<int> onMsg = delegate
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
            var exec = new StubExecutor();
            var one = new PoolFiber(exec);
            one.Start();
            var reset = new AutoResetEvent(false);
            using (one)
            {
                var channel = new QueueChannel<int>();
                Action<int> onMsg = delegate(int num)
                {
                    if (num == 0)
                        throw new Exception();
                    reset.Set();
                };
                channel.Subscribe(one, onMsg);
                channel.Publish(0);
                channel.Publish(1);
                Assert.IsTrue(reset.WaitOne(10000, false));
                Assert.AreEqual(1, exec.failed.Count);
            }
        }
    }

    public class StubExecutor : Executor
    {
        public List<Exception> failed = new List<Exception>();

        public override void Execute(IEnumerable<Action> toExecute)
        {
            foreach (Action action in toExecute)
                Execute(action);
        }

        public override void Execute(Action toExecute)
        {
            try
            {
                toExecute();
            }
            catch (Exception e)
            {
                failed.Add(e);
            }
        }
    }
}