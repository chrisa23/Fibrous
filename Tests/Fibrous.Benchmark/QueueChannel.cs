using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class QueueChannel
    {
        private const int OperationsPerInvoke = 1_000_000;
        private static readonly Stopwatch _sw = Stopwatch.StartNew();
        private readonly Random _rnd = new();
        [Params(2, 3, 4, 5)] public int N;
        [Params(0, 1)] public int Wait;

        private static void NOP(double durationMS)
        {
            double durationTicks = Math.Round(durationMS * Stopwatch.Frequency / 1000) + _sw.ElapsedTicks;

            while (_sw.ElapsedTicks < durationTicks)
            {
            }
        }

        private void RunMult(IFiberFactory factory, Func<IChannel<int>> queueFactory, int count, int wait1)
        {
            using AutoResetEvent wait = new(false);
            int hCount = 0;

            void Handler(int s)
            {
                int c = Interlocked.Increment(ref hCount);
                if (c == OperationsPerInvoke)
                {
                    wait.Set();
                }

                NOP(wait1 / 1000.0);
            }

            using IChannel<int> queue = queueFactory();
            using IDisposable fibers = new Disposables(Enumerable.Range(0, count).Select(x =>
            {
                IFiber fiber = factory.CreateFiber();
                IDisposable sub = queue.Subscribe(fiber, Handler);
                return fiber;
            }));
            for (int j = 1; j <= OperationsPerInvoke; j++)
            {
                queue.Publish(j);
            }

            WaitHandle.WaitAny(new WaitHandle[] {wait});
        }

        public void RunMultAsync(IFiberFactory factory, Func<IChannel<int>> queueFactory, int count, int wait1)
        {
            using AutoResetEvent wait = new(false);
            int hCount = 0;

            Task AsyncHandler(int s)
            {
                int c = Interlocked.Increment(ref hCount);
                if (c == OperationsPerInvoke)
                {
                    wait.Set();
                }

                NOP(wait1 / 1000.0);
                return Task.CompletedTask;
            }

            using IChannel<int> _queue = queueFactory();
            using IDisposable fibers = new Disposables(Enumerable.Range(0, count).Select(x =>
            {
                IAsyncFiber fiber = factory.CreateAsyncFiber(ex => { });
                IDisposable sub = _queue.Subscribe(fiber, AsyncHandler);
                return fiber;
            }));
            for (int j = 1; j <= OperationsPerInvoke; j++)
            {
                _queue.Publish(j);
            }

            WaitHandle.WaitAny(new WaitHandle[] {wait});
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Fiber() => RunMult(new FiberFactory(), () => new QueueChannel<int>(), N, Wait);

        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void Fiber3()
        //{
        //    RunMult(new FiberFactory(), () => new QueueChannel3<int>(), N, Wait);
        //}
        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void Fiber2()
        //{
        //    RunMult(new FiberFactory(), () => new QueueChannel2<int>(), N, Wait);
        //}
        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void FiberRR()
        //{
        //    RunMult(new FiberFactory(), () => new QueueChannelRR<int>(), N, Wait);
        //}
        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void FiberRR2()
        //{
        //    RunMult(new FiberFactory(), () => new QueueChannelRR2<int>(), N, Wait);
        //}

        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void FiberRR3()
        //{
        //    RunMult(new FiberFactory(), () => new QueueChannelRR3<int>(), N, Wait);

        //}

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Async() => RunMultAsync(new FiberFactory(), () => new QueueChannel<int>(), N, Wait);

        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void Async3()
        //{
        //    RunMultAsync(new FiberFactory(), () => new QueueChannel3<int>(), N, Wait);
        //}

        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void Async2()
        //{
        //    RunMultAsync(new FiberFactory(), () => new QueueChannel2<int>(), N, Wait);
        //}
        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void AsyncRR()
        //{
        //    RunMultAsync(new FiberFactory(), () => new QueueChannelRR<int>(), N, Wait);
        //}
        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void AsyncRR2()
        //{
        //    RunMultAsync(new FiberFactory(), () => new QueueChannelRR2<int>(), N, Wait);
        //}
    }
}
