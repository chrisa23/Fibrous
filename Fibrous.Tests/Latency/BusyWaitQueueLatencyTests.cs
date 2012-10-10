namespace Fibrous.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using Fibrous.Fibers.Queues;
    using NUnit.Framework;

    [TestFixture]
    public class BusyWaitQueueLatencyTests
    {
        [Test]
        [Explicit]
        public void CompareBusyWaitQueueVsDefaultQueueLatency()
        {
            Func<ThreadFiber> blocking = () => new ThreadFiber();
            Func<ThreadFiber> polling = () => new ThreadFiber(new BusyWaitQueue(100000, 30000));
            for (int i = 0; i < 20; i++)
            {
                Execute(blocking, "Blocking");
                Execute(polling, "Polling");
                Console.WriteLine();
            }
        }

        private static void Execute(Func<ThreadFiber> creator, String name)
        {
            Console.WriteLine(name);
            const int channelCount = 5;
            double msPerTick = 1000.0 / Stopwatch.Frequency;
            var channels = new IChannel<Msg>[channelCount];
            for (int i = 0; i < channels.Length; i++)
                channels[i] = new Channel<Msg>();
            var fibers = new ThreadFiber[channelCount];
            for (int i = 0; i < fibers.Length; i++)
            {
                fibers[i] = creator();
                fibers[i].Start();
                int prior = i - 1;
                bool isLast = i + 1 == fibers.Length;
                IChannel<Msg> target = !isLast ? channels[i] : null;
                if (prior >= 0)
                {
                    Action<Msg> cb = delegate(Msg message)
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
                    };
                    channels[prior].Subscribe(fibers[i], cb);
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
            public readonly bool Log;
            public readonly long Time = Stopwatch.GetTimestamp();
            public readonly ManualResetEvent Latch = new ManualResetEvent(false);

            public Msg(bool log)
            {
                Log = log;
            }
        }
    }
}