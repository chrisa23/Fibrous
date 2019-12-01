using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
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
                    for (int j = 0; j < 3; j++)
                    {
                    //Warmup
                    for (var i = 0; i <= Max; i++)
                        channel.Publish(new MsgStruct {Count = i});
                    reset.WaitOne(30000);

                    using (new PerfTimer(Max))
                    {
                        for (var i = 0; i <= Max; i++)
                            channel.Publish(new MsgStruct {Count = i});
                        Assert.IsTrue(reset.WaitOne(30000, false));
                    }
                }
            }
        }

        private static void PointToPointPerfTestWithStruct(IAsyncFiber fiber)
        {
            using (fiber)
            {
                fiber.Start();
                IChannel<MsgStruct> channel = new Channel<MsgStruct>();
                const int Max = 5000000;
                var reset = new AutoResetEvent(false);
                var counter = new AsyncCounter(reset, Max);
                channel.Subscribe(fiber, counter.OnMsg);
                //Warmup
                for (var i = 0; i <= Max; i++)
                    channel.Publish(new MsgStruct {Count = i});
                reset.WaitOne(30000);
                using (new PerfTimer(Max))
                {
                    for (var i = 0; i <= Max; i++)
                        channel.Publish(new MsgStruct {Count = i});
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }

        private struct AsyncCounter
        {
            private readonly int _cutoff;
            private readonly AutoResetEvent _handle;

            public AsyncCounter(AutoResetEvent handle, int cutoff)
            {
                _handle = handle;
                _cutoff = cutoff;
                Count = 0;
            }

            private int Count { get; set; }

            public Task OnMsg(MsgStruct msg)
            {
                Count++;
                if (Count == _cutoff)
                {
                    _handle.Set();
                    Count = 0;
                }

                return Task.CompletedTask;
            }
        }

        private struct Counter
        {
            private readonly int _cutoff;
            private readonly AutoResetEvent _handle;

            public Counter(AutoResetEvent handle, int cutoff)
            {
                _handle = handle;
                _cutoff = cutoff;
                Count = 0;
            }

            private int Count { get; set; }

            public void OnMsg(MsgStruct msg)
            {
                Count++;
                if (Count == _cutoff)
                {
                    _handle.Set();
                    Count = 0;
                }
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

        public void PointToPointPerfTestWithInt(IFiber fiber)
        {
            using (fiber)
            {
                fiber.Start();
                var channel = new Channel<int>();
                const int Max = 5000000;
                var reset = new AutoResetEvent(false);
                var counter = new CounterInt(reset, Max);
                channel.Subscribe(fiber, counter.OnMsg);
                for (var i = 0; i <= Max; i++)
                    channel.Publish(i);
                reset.WaitOne(30000);
                using (new PerfTimer(Max))
                {
                    for (var i = 0; i <= Max; i++)
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
                var msg = new object();
                for (var i = 0; i <= Max; i++)
                    channel.Publish(msg);
                channel.Publish(end);
                reset.WaitOne(30000);
                using (new PerfTimer(Max))
                {
                    for (var i = 0; i <= Max; i++)
                        channel.Publish(msg);
                    channel.Publish(end);
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }

        public void PointToPointPerfTestWithObject(IAsyncFiber fiber)
        {
            using (fiber)
            {
                fiber.Start();
                var channel = new Channel<object>();
                const int Max = 5000000;
                var reset = new AutoResetEvent(false);
                var end = new object();

                Task OnMsg(object msg1)
                {
                    if (msg1 == end)
                        reset.Set();
                    return Task.CompletedTask;
                }

                channel.Subscribe(fiber, OnMsg);
                var msg = new object();
                for (var i = 0; i <= Max; i++)
                    channel.Publish(msg);
                channel.Publish(end);
                reset.WaitOne(30000);
                using (new PerfTimer(Max))
                {
                    for (var i = 0; i <= Max; i++)
                        channel.Publish(msg);
                    channel.Publish(end);
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }

        [Test]
        //[Explicit]
        public void TestAsync()
        {
            PointToPointPerfTestWithObject(new AsyncFiber());
            PointToPointPerfTestWithStruct(new AsyncFiber());
        }

        [Test]
       // [Explicit]
        public void TestPool()
        {
            PointToPointPerfTestWithStruct(new PoolFiber());
            PointToPointPerfTestWithInt(new PoolFiber());
            PointToPointPerfTestWithObject(new PoolFiber());
        }
    }
}