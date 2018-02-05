namespace Fibrous.Benchmark
{
    using System;
    using BenchmarkDotNet.Attributes;
    using Fibrous.Fibers;
    using Fibrous.Queues;

    [MemoryDiagnoser]
    public class Queues
    {
        private IFiber _yield;
        private IFiber _sleep;
        private IFiber _spin;
        private IFiber _busyWait;
        private readonly Action _lambda = () => { };
        
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
            _busyWait = new ThreadFiber(new Executor(), new BusyWaitQueue(1000,100));
            _busyWait.Start();
        }

        [IterationCleanup(Target = "BusyWait")]
        public void BusyWaitCleanup()
        {
            _busyWait.Dispose();
        }
    }
}