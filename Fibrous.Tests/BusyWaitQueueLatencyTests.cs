namespace Fibrous.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using Fibrous.Queues;
    using Fibrous.Scheduling;
    using NUnit.Framework;

    [TestFixture]
    public class BusyWaitQueueLatencyTests
    {
        private static void Execute(Func<ThreadFiber> creator, String name)
        {
            Console.WriteLine(name);
            const int ChannelCount = 5;
            double msPerTick = 1000.0 / Stopwatch.Frequency;
            var channels = new IChannel<Msg>[ChannelCount];
            for (int i = 0; i < channels.Length; i++)
                channels[i] = new Channel<Msg>();
            var fibers = new ThreadFiber[ChannelCount];
            for (int i = 0; i < fibers.Length; i++)
            {
                fibers[i] = creator();
                fibers[i].Start();
                int prior = i - 1;
                bool isLast = i + 1 == fibers.Length;
                IChannel<Msg> target = !isLast ? channels[i] : null;
                if (prior >= 0)
                {
                    void Cb(Msg message)
                    {
                        if (target != null)
                            target.Publish(message);
                        else
                        {
                            long now = Stopwatch.GetTimestamp();
                            long diff = now - message.Time;
                            if (message.Log)
                                Console.WriteLine("qTime: " + diff * msPerTick);
                            message.Latch.Set();
                        }
                    }

                    channels[prior].Subscribe(fibers[i], Cb);
                }
            }
            for (int i = 0; i < 10000; i++)
            {
                var s = new Msg(false);
                channels[0].Publish(s);
                s.Latch.WaitOne();
            }
            for (int i = 0; i < 5; i++)
            {
                var s = new Msg(true);
                channels[0].Publish(s);
                Thread.Sleep(10);
            }
            foreach (ThreadFiber fiber in fibers)
                fiber.Dispose();
        }

        private class Msg
        {
            public readonly ManualResetEvent Latch = new ManualResetEvent(false);
            public readonly bool Log;
            public readonly long Time = Stopwatch.GetTimestamp();

            public Msg(bool log)
            {
                Log = log;
            }
        }

        [Test]
        [Explicit]
        public void CompareBusyWaitQueueVsDefaultQueueLatency()
        {
            for (int i = 0; i < 3; i++)
            {
                ThreadFiber Blocking() => new ThreadFiber(new Executor(), new BoundedQueue(1000));
                ThreadFiber Polling() => new ThreadFiber(new Executor(), new TimerScheduler(), new BusyWaitQueue(100000, 30000), "");
                ThreadFiber Yielding() => new ThreadFiber(new Executor(), new TimerScheduler(), new YieldingQueue(), "");
                ThreadFiber Sleeping() => new ThreadFiber(new Executor(), new TimerScheduler(), new SleepingQueue(), "");
                Execute(Blocking, "Blocking");
                Execute(Polling, "Polling");
                Execute(Yielding, "Yielding");
                Execute(Sleeping, "Sleeping");
                Thread.Sleep(100);
                Console.WriteLine();
            }
        }
    }
}