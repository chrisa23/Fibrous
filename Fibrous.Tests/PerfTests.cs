namespace Fibrous.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Channels;
    using Fibrous.Experimental;
    using Fibrous.Fibers;
    using Fibrous.Queues;
    using Fibrous.Scheduling;
    using NUnit.Framework;
    
    public sealed class PerfExecutor : IExecutor
    {
        public void Execute(List<Action> toExecute)
        {
            int count = 0;
            for (int index = 0; index < toExecute.Count; index++)
            {
                Action action = toExecute[index];
                action();
                count++;
            }
            if (count < 10000)
                Thread.Sleep(1);
        }

        public void Execute(Action toExecute)
        {
            toExecute();
        }
    }

    public struct MsgStruct
    {
        public int Count;
    }

    [TestFixture]
    public class PerfTests
    {
        private static void PointToPointPerfTestWithStruct(IFiber fiber)
        {
            using (fiber)
            {
                fiber.Start();
                IChannel<MsgStruct> channel = new Channel<MsgStruct>();
                const int Max = 5000000;
                var reset = new AutoResetEvent(false);
                var counter = new Counter(reset, Max);
                channel.Subscribe(fiber, counter.OnMsg);
                Thread.Sleep(100);
                //Warmup
                for (int i = 0; i <= Max; i++)
                    channel.Publish(new MsgStruct { Count = i });
                using (new PerfTimer(Max))
                {
                    for (int i = 0; i <= Max; i++)
                        channel.Publish(new MsgStruct { Count = i });
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }

        private sealed class Counter
        {
            private readonly int _cutoff;
            private readonly AutoResetEvent _handle;

            public Counter(AutoResetEvent handle, int cutoff)
            {
                _handle = handle;
                _cutoff = cutoff;
            }

            private int Count { get; set; }

            public void OnMsg(MsgStruct msg)
            {
                Count++;
                if (Count == _cutoff)
                    _handle.Set();
            }
        }

        private sealed class CounterInt
        {
            private readonly int _cutoff;
            private readonly AutoResetEvent _handle;

            public CounterInt(AutoResetEvent handle, int cutoff)
            {
                _handle = handle;
                _cutoff = cutoff;
            }

            public void OnMsg(int msg)
            {
                if (msg == _cutoff)
                    _handle.Set();
            }
        }

        public void PointToPointPerfTestWithInt(FiberBase fiber)
        {
            using (fiber)
            {
                fiber.Start();
                var channel = new Channel<int>();
                const int Max = 5000000;
                var reset = new AutoResetEvent(false);
                var counter = new CounterInt(reset, Max);
                channel.Subscribe(fiber, counter.OnMsg);
                Thread.Sleep(100);
                for (int i = 0; i <= Max; i++)
                    channel.Publish(i);
                using (new PerfTimer(Max))
                {
                    for (int i = 0; i <= Max; i++)
                        channel.Publish(i);
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }

        public void PointToPointPerfTestWithObject(IFiber fiber)
        {
            using (fiber)
            {
                fiber.Start();
                var channel = new Channel<object>();
                const int Max = 5000000;
                var reset = new AutoResetEvent(false);
                var end = new object();

                void OnMsg(object msg1)
                {
                    if (msg1 == end)
                        reset.Set();
                }

                channel.Subscribe(fiber, OnMsg);
                Thread.Sleep(100);
                var msg = new object();
                for (int i = 0; i <= Max; i++)
                    channel.Publish(msg);
                using (new PerfTimer(Max))
                {
                    for (int i = 0; i <= Max; i++)
                        channel.Publish(msg);
                    channel.Publish(end);
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }

       

        [Test]
        [Explicit]
        public void TestBoundedQueue()
        {
            PointToPointPerfTestWithStruct(new ThreadFiber(new PerfExecutor(), new TimerScheduler(), new BoundedQueue(1000), ""));
            PointToPointPerfTestWithInt(new ThreadFiber(new PerfExecutor(), new TimerScheduler(), new BoundedQueue(1000), ""));
            PointToPointPerfTestWithObject(new ThreadFiber(new PerfExecutor(), new TimerScheduler(), new BoundedQueue(1000), ""));
        }

        [Test]
        [Explicit]
        public void TestBusyWait()
        {
            PointToPointPerfTestWithStruct(new ThreadFiber(new PerfExecutor(), new TimerScheduler(), new BusyWaitQueue(1000, 25), ""));
            PointToPointPerfTestWithInt(new ThreadFiber(new PerfExecutor(), new TimerScheduler(), new BusyWaitQueue(1000, 25), ""));
            PointToPointPerfTestWithObject(new ThreadFiber(new PerfExecutor(), new TimerScheduler(), new BusyWaitQueue(1000, 25), ""));
        }

        [Test]
        [Explicit]
        public void TestDefault()
        {
            PointToPointPerfTestWithStruct(new ThreadFiber(new PerfExecutor()));
            PointToPointPerfTestWithInt(new ThreadFiber(new PerfExecutor()));
            PointToPointPerfTestWithObject(new ThreadFiber(new PerfExecutor()));
        }

        [Test]
        [Explicit]
        public void TestDefaultSleep()
        {
            PointToPointPerfTestWithStruct(new ThreadFiber(new PerfExecutor(), new SleepingQueue()));
            PointToPointPerfTestWithInt(new ThreadFiber(new PerfExecutor(), new SleepingQueue()));
            PointToPointPerfTestWithObject(new ThreadFiber(new PerfExecutor(), new SleepingQueue()));
        }

        [Test]
        [Explicit]
        public void TestDefaultSleep2()
        {
            PointToPointPerfTestWithStruct(new ThreadFiber(new Executor(), new SleepingQueue()));
            PointToPointPerfTestWithInt(new ThreadFiber(new Executor(), new SleepingQueue()));
            PointToPointPerfTestWithObject(new ThreadFiber(new Executor(), new SleepingQueue()));
        }

        [Test]
        [Explicit]
        public void TestDefaultYield()
        {
            PointToPointPerfTestWithStruct(new ThreadFiber(new PerfExecutor(), new YieldingQueue()));
            PointToPointPerfTestWithInt(new ThreadFiber(new PerfExecutor(), new YieldingQueue()));
            PointToPointPerfTestWithObject(new ThreadFiber(new PerfExecutor(), new YieldingQueue()));
        }

        [Test]
        [Explicit]
        public void TestSpinLock()
        {
            PointToPointPerfTestWithStruct(new ThreadFiber(new PerfExecutor(), new SpinLockQueue()));
            PointToPointPerfTestWithInt(new ThreadFiber(new PerfExecutor(), new SpinLockQueue()));
            PointToPointPerfTestWithObject(new ThreadFiber(new PerfExecutor(), new SpinLockQueue()));
        }

        //[Test]
        //[Explicit]
        //public void TestSpinWait()
        //{
        //    PointToPointPerfTestWithStruct(new ThreadFiber(new PerfExecutor(), new SpinWaitQueue()));
        //    PointToPointPerfTestWithInt(new ThreadFiber(new PerfExecutor(), new SpinWaitQueue()));
        //    PointToPointPerfTestWithObject(new ThreadFiber(new PerfExecutor(), new SpinWaitQueue()));
        //}

        [Test]
        [Explicit]
        public void TestPool()
        {
            PointToPointPerfTestWithStruct(new PoolFiber(new PerfExecutor()));
            PointToPointPerfTestWithInt(new PoolFiber(new PerfExecutor()));
            PointToPointPerfTestWithObject(new PoolFiber(new PerfExecutor()));
        }

        [Test]
        [Explicit]
        public void TestSpinLockPool()
        {
            PointToPointPerfTestWithStruct(new SpinLockPoolFiber(new PerfExecutor()));
            PointToPointPerfTestWithInt(new SpinLockPoolFiber(new PerfExecutor()));
            PointToPointPerfTestWithObject(new SpinLockPoolFiber(new PerfExecutor()));
        }

        [Test]
        [Explicit]
        public void TestPool2()
        {
            PointToPointPerfTestWithStruct(new PoolFiber2(new PerfExecutor()));
            PointToPointPerfTestWithInt(new PoolFiber2(new PerfExecutor()));
            PointToPointPerfTestWithObject(new PoolFiber2(new PerfExecutor()));
        }

        [Test]
        [Explicit]
        public void TestBlocking()
        {
            PointToPointPerfTestWithStruct(new ThreadFiber(new PerfExecutor(), new BlockingQueue()));
            PointToPointPerfTestWithInt(new ThreadFiber(new PerfExecutor(), new BlockingQueue()));
            PointToPointPerfTestWithObject(new ThreadFiber(new PerfExecutor(), new BlockingQueue()));
        }
    }
}