namespace Fibrous.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using Fibrous.Queues;
    using Fibrous.Scheduling;
    using NUnit.Framework;

    [TestFixture]
    public class QueueChannelTests
    {
        [Test]
        public void Multiple()
        {
            var queues = new List<IFiber>();
            int receiveCount = 0;
            using (var reset = new AutoResetEvent(false))
            {
                var channel = new QueueChannel<int>();
                const int MessageCount = 100;
                var updateLock = new object();
                for (int i = 0; i < 5; i++)
                {
                    void OnReceive(int obj)
                    {
                        Thread.Sleep(15);
                        lock (updateLock)
                        {
                            receiveCount++;
                            if (receiveCount == MessageCount)
                                reset.Set();
                        }
                    }

                    IFiber fiber = PoolFiber.StartNew();
                    queues.Add(fiber);
                    channel.Subscribe(fiber, OnReceive);
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
            using (IFiber one = PoolFiber.StartNew())
            using (var reset = new AutoResetEvent(false))
            {
                var channel = new QueueChannel<int>();

                void OnMsg(int obj)
                {
                    oneConsumed++;
                    if (oneConsumed == 20)
                        reset.Set();
                }

                channel.Subscribe(one, OnMsg);
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
            using (IFiber one = PoolFiber.StartNew(exec))
            using (var reset = new AutoResetEvent(false))
            {
                var channel = new QueueChannel<int>();

                void OnMsg(int num)
                {
                    if (num == 0)
                        throw new Exception();
                    reset.Set();
                }

                channel.Subscribe(one, OnMsg);
                channel.Publish(0);
                channel.Publish(1);
                Assert.IsTrue(reset.WaitOne(10000, false));
                Assert.AreEqual(1, failed.Count);
            }
        }

        [Test]
        public void MultiConsumer()
        {
            var queues = new List<IFiber>();
            IChannel<string> channel = new QueueChannel<string>();

            //Init executing Fibers
            for (int i = 0; i < 5; i++)
            {
                char OnReceive(string message)
                {
                    return message[0];
                }

                IFiber threadFiber = new ThreadFiber(new Executor(), new TimerScheduler(), new SleepingQueue(), i.ToString());
                threadFiber.Start();
                queues.Add(threadFiber);
                channel.Subscribe(threadFiber, x => OnReceive(x));
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //Push messages
            for (int i = 0; i < 1000000; i++)
            {
                string msg = "[" + i + "] Push";
                channel.Publish(msg);
            }
            sw.Stop();
            Console.WriteLine("End : " + sw.ElapsedMilliseconds);
            // Console.ReadLine();

            //#Results:
            //1 ThreadFiber ~= 1sec
            //2 ThreadFiber ~=> 3sec
            //3 ThreadFiber ~=> 5sec
            //4 ThreadFiber ~=> 8sec
            //5 ThreadFiber ~=> 10sec
        }

        [Test]
        public void MultiConsumerYielding()
        {
            var queues = new List<IFiber>();
            IChannel<string> channel = new QueueChannel<string>();
            int o = 0;
            //Init executing Fibers
            for (int i = 0; i < 5; i++)
            {
                void OnReceive(string message)
                {
                    var firstChar = message[0];
                    if (firstChar == firstChar) o++;
                }

                IFiber threadFiber =
                    new ThreadFiber(new Executor(), new TimerScheduler(), new YieldingQueue(), i.ToString()); 
                threadFiber.Start();
                queues.Add(threadFiber);
                channel.Subscribe(threadFiber, OnReceive);
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //Push messages
            for (int i = 0; i < 1000000; i++)
            {
                string msg = "[" + i + "] Push";
                channel.Publish(msg);
            }
            sw.Stop();
            Console.WriteLine("End : " + sw.ElapsedMilliseconds);
            // Console.ReadLine();

            //#Results:
            //1 ThreadFiber ~= 1sec
            //2 ThreadFiber ~=> 3sec
            //3 ThreadFiber ~=> 5sec
            //4 ThreadFiber ~=> 8sec
            //5 ThreadFiber ~=> 10sec
        }

        [Test]
        public void MultiConsumerPool()
        {
            var queues = new List<IFiber>();
            IChannel<string> channel = new QueueChannel<string>();

            //Init executing Fibers
            for (int i = 0; i < 10; i++)
            {
                void OnReceive(string message)
                {
                    var firstChar = message[0];
                }

                IFiber fiber = PoolFiber.StartNew(); 
                queues.Add(fiber);
                channel.Subscribe(fiber, OnReceive);
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //Push messages
            for (int i = 0; i < 1000000; i++)
            {
                string msg = "[" + i + "] Push";
                channel.Publish(msg);
            }
            sw.Stop();
            Console.WriteLine("End : " + sw.ElapsedMilliseconds);

            //#Results:
            //1 ThreadFiber ~= 1sec
            //2 ThreadFiber ~=> 3sec
            //3 ThreadFiber ~=> 5sec
            //4 ThreadFiber ~=> 8sec
            //5 ThreadFiber ~=> 10sec
        }

        [Test]
        public void OnlyOneConsumer()
        {
            using (var queues = new Disposables())
            {
                IChannel<int> channel = new QueueChannel<int>();
                ConcurrentBag<int> results = new ConcurrentBag<int>();

                void OnReceive(int message)
                {
                    results.Add(message);
                }

                //Init executing Fibers
                for (int i = 0; i < 10; i++)
                {
                    IFiber threadFiber = ThreadFiber.StartNew();
                    queues.Add(threadFiber);
                    channel.Subscribe(threadFiber, OnReceive);
                }
                //Push messages
                for (int i = 0; i < 1000000; i++)
                {
                    channel.Publish(i);
                }

                Thread.Sleep(100);
                Assert.AreEqual(1000000, results.Count);
                var r1 = results.OrderBy(x => x).ToArray();
                for (int i = 0; i < 1000000; i++)
                {
                    Assert.AreEqual(i, r1[i]);
                }
            }
        }
    }
}