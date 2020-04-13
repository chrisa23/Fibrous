using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class QueueChannelTests
    {
        private static void NOP(double durationMS)
        {
            var durationTicks = Math.Round(durationMS * Stopwatch.Frequency/1000);
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedTicks < durationTicks)
            {

            }
        }
        private void QueueChannelTest(int fibers, Func<IFiber> factory, int messageCount, Func<IChannel<int>> channelFactory)
        {
            using var queues = new Disposables();
            using var reset = new AutoResetEvent(false);
            using var channel = channelFactory();
            int count = 0;
            void OnReceive(int obj)
            {
                int c = Interlocked.Increment(ref count);
                if (c == messageCount)
                    reset.Set();
                NOP(1);
            }

            for (var i = 0; i < fibers; i++)
            {
                var fiber = factory();
                queues.Add(fiber);
                channel.Subscribe(fiber, OnReceive);
            }

            var sw = Stopwatch.StartNew();

            //Push messages
            for (var i = 1; i <= messageCount; i++) 
                channel.Publish(i);
            
            Assert.IsTrue(reset.WaitOne(10000, false));
            sw.Stop();
            Console.WriteLine($"Fibers: {fibers}  End : {sw.ElapsedMilliseconds} Count {count}");
        }


        [Test]
        public void Multiple()
        {
            const int MessageCount = 100;
            var queues = new List<IFiber>();
            var receiveCount = 0;
            using var reset = new AutoResetEvent(false);
            var updateLock = new object();

            void OnReceive(int obj)
            {
                Thread.Sleep(15);
                lock (updateLock)
                {
                    Interlocked.Increment(ref receiveCount);
                    if (receiveCount == MessageCount)
                        reset.Set();
                }
            }

            var channel = new QueueChannel<int>();

            for (var i = 0; i < 5; i++)
            {
                var fiber = new Fiber();
                queues.Add(fiber);
                channel.Subscribe(fiber, OnReceive);
            }

            for (var i = 0; i < MessageCount; i++)
                channel.Publish(i);
            Assert.IsTrue(reset.WaitOne(10000, false));
            queues.ForEach(q => q.Dispose());
        }

        [Test]
        public void PoolQueue()
        {
            
            var msgCount = 1000;
            IFiber Factory() => new Fiber();
            //for (int i = 0; i < 10; i++) QueueChannelTest(1, Factory, msgCount, () => new Channel<int>());

            Console.WriteLine("First");

            for (var i = 2; i < 10; i++) QueueChannelTest(i, Factory, msgCount, () => new QueueChannel<int>());

            //Console.WriteLine("Second");
            //for (var i = 1; i < 10; i++) QueueChannelTest(i, Factory, msgCount, () => new QueueChannel2<int>());

            //Console.WriteLine("RR");
            //for (var i = 1; i < 10; i++) QueueChannelTest(i, Factory, msgCount, () => new QueueChannelRR<int>());

            //Console.WriteLine("RR2");
            //for (var i = 1; i < 10; i++) QueueChannelTest(i, Factory, msgCount, () => new QueueChannelRR2<int>());

            //Console.WriteLine("RR3");
            //for (var i = 1; i < 10; i++) QueueChannelTest(i, Factory, msgCount, () => new QueueChannelRR3<int>());
        }

        [Test]
        public void SingleConsumer()
        {
            var oneConsumed = 0;
            using var one = new Fiber();
            using var reset = new AutoResetEvent(false);
            var channel = new QueueChannel<int>();

            void OnMsg(int obj)
            {
                Interlocked.Increment(ref oneConsumed);
                if (oneConsumed == 20)
                    reset.Set();
            }

            channel.Subscribe(one, OnMsg);
            for (var i = 0; i < 20; i++)
                channel.Publish(i);
            Assert.IsTrue(reset.WaitOne(10000, false));
        }

        //[Test]
        //public void SingleConsumer2()
        //{
        //    var oneConsumed = 0;
        //    using var one = new Fiber();
        //    using var reset = new AutoResetEvent(false);
        //    var channel = new QueueChannel2<int>();

        //    void OnMsg(int obj)
        //    {
        //        Interlocked.Increment(ref oneConsumed);
        //        if (oneConsumed == 20)
        //            reset.Set();
        //    }

        //    channel.Subscribe(one, OnMsg);
        //    for (var i = 0; i < 20; i++)
        //        channel.Publish(i);
        //    Assert.IsTrue(reset.WaitOne(10000, false));
        //}

        [Test]
        public void SingleConsumerWithException()
        {
            using var reset = new AutoResetEvent(false);

            void OnMsg(int num)
            {
                if (num == 0)
                    throw new Exception();
                reset.Set();
            }

            var failed = new List<Exception>();
            var exec = new ExceptionHandlingExecutor(failed.Add);
            using var one = new Fiber(exec);
            var channel = new QueueChannel<int>();
            channel.Subscribe(one, OnMsg);
            channel.Publish(0);
            channel.Publish(1);
            Assert.IsTrue(reset.WaitOne(10000, false));
            Assert.AreEqual(1, failed.Count);
        }

        [Test]
        public void FullDrain()
        {
            using var reset = new AutoResetEvent(false);
            int count = 0;

            void OnMessage(int i)
            {
                int c = Interlocked.Increment(ref count);
                Thread.Sleep(100);
                if (c == 20)
                    reset.Set();
            }

            using var fiber = new Fiber();
            using var fiber2 = new Fiber();
            using var queue = new QueueChannel<int>();
            queue.Subscribe(fiber, OnMessage);
            queue.Subscribe(fiber2, OnMessage);
            for (int i = 0; i < 20; i++)
            {
                queue.Publish(i);
            }

            Assert.IsTrue(reset.WaitOne(15000, false));
            Assert.AreEqual(20, count);
        }

        [Test]
        public void FullDrain2()
        {
            const int Max = 1_000_000;
            using var reset = new AutoResetEvent(false);
            int count = 0;

            void OnMessage(int i)
            {
                int c = Interlocked.Increment(ref count);
                if (c == Max)
                    reset.Set();
            }

            using var fiber = new Fiber();
            using var fiber2 = new Fiber();
            using var queue = new QueueChannel<int>();
            queue.Subscribe(fiber, OnMessage);
            queue.Subscribe(fiber2, OnMessage);
            for (int i = 0; i < Max; i++)
            {
                queue.Publish(i);
            }

            Assert.IsTrue(reset.WaitOne(15000, false));
            Assert.AreEqual(Max, count);
        }

        //[Test]
        //public void FullDrain3()
        //{
        //    const int Max = 1_000_000;
        //    using var reset = new AutoResetEvent(false);
        //    int count = 0;

        //    void OnMessage(int i)
        //    {
        //        int c = Interlocked.Increment(ref count);
        //        if (c == Max)
        //            reset.Set();
        //    }

        //    using var fiber = new Fiber();
        //    using var fiber2 = new Fiber();
        //    using var queue = new QueueChannelRR2<int>();
        //    queue.Subscribe(fiber, OnMessage);
        //    queue.Subscribe(fiber2, OnMessage);
        //    for (int i = 0; i < Max; i++)
        //    {
        //        queue.Publish(i);
        //    }

        //    Assert.IsTrue(reset.WaitOne(15000, false));
        //    Assert.AreEqual(Max, count);
        //}

        [Test]
        public void WorkDistribution()
        {
            int count = 0;
            int count2 = 0;

            void OnMessage(int i)
            {
                count++;
                Thread.Sleep(100);
            }

            void OnMessage2(int i)
            {
                count2++;
                Thread.Sleep(100);
            }

            using var fiber = new Fiber();
            using var fiber2 = new Fiber();
            using var queue = new QueueChannel<int>();
            queue.Subscribe(fiber, OnMessage);
            queue.Subscribe(fiber2, OnMessage2);
            for (int i = 0; i < 20; i++)
            {
                queue.Publish(i);
            }

            Thread.Sleep(10000);
            Console.WriteLine($"{count} | {count2}");
            Assert.AreEqual(10, count);
            Assert.AreEqual(10, count2);
        }

        [Test]
        public void ThreeAsync()
        {
            int OperationsPerInvoke = 1000000;
            AutoResetEvent wait = new AutoResetEvent(false);
            int count = 0;

            Task AsyncHandler(int s)
            {
                int i = Interlocked.Increment(ref count);
                if (i == OperationsPerInvoke)
                    wait.Set();
                return Task.CompletedTask;
            }

            using IChannel<int> _queue = new QueueChannel<int>();
            using var fiber1 = new AsyncFiber();
            using var fiber2 = new AsyncFiber();
            using var fiber3 = new AsyncFiber();
            using var sub = _queue.Subscribe(fiber1, AsyncHandler);
            using var sub2 = _queue.Subscribe(fiber2, AsyncHandler);
            using var sub3 = _queue.Subscribe(fiber3, AsyncHandler);

            for (var j = 0; j < OperationsPerInvoke; j++) _queue.Publish(j);
            Assert.IsTrue(wait.WaitOne(15000, false));
        }

    }
}