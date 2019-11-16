using System;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class Queues
    {
        private readonly Action _lambda = () => { };
        private IFiber _busyWait;
        private IFiber _sleep;
        private IFiber _spin;
        private IFiber _yield;

        [Benchmark]
        public void Yield()
        {
            _yield.Enqueue(_lambda);
        }

        [IterationSetup(Target = "Yield")]
        public void YieldSetup()
        {
            _yield = new ThreadFiber(new Executor(), new YieldingQueue());
            _yield.Start();
        }

        [IterationCleanup(Target = "Yield")]
        public void Cleanup()
        {
            _yield.Dispose();
        }

        [Benchmark]
        public void Spin()
        {
            _spin.Enqueue(_lambda);
        }

        [IterationSetup(Target = "Spin")]
        public void SpinSetup()
        {
            _spin = new ThreadFiber(new Executor(), new SpinLockQueue());
            _spin.Start();
        }

        [IterationCleanup(Target = "Spin")]
        public void SpinCleanup()
        {
            _spin.Dispose();
        }

        [Benchmark]
        public void Sleep()
        {
            _sleep.Enqueue(_lambda);
        }

        [IterationSetup(Target = "Sleep")]
        public void SleepSetup()
        {
            _sleep = new ThreadFiber(new Executor(), new SleepingQueue());
            _sleep.Start();
        }

        [IterationCleanup(Target = "Sleep")]
        public void SleepCleanup()
        {
            _sleep.Dispose();
        }

        [Benchmark]
        public void BusyWait()
        {
            _busyWait.Enqueue(_lambda);
        }

        [IterationSetup(Target = "BusyWait")]
        public void BusyWaitSetup()
        {
            _busyWait = new ThreadFiber(new Executor(), new BusyWaitQueue(1000, 100));
            _busyWait.Start();
        }

        [IterationCleanup(Target = "BusyWait")]
        public void BusyWaitCleanup()
        {
            _busyWait.Dispose();
        }
    }
}