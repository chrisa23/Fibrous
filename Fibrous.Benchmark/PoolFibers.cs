using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fibrous.Benchmark
{
    using System.Threading;
    using BenchmarkDotNet.Attributes;
    using Fibrous.Experimental;
    using Fibrous.Fibers;
    using Fibrous.Queues;

    public class PoolFibers
    {
        private IFiber _pool1;
        private IFiber _pool2;
        private IFiber _spinPool;
        private AutoResetEvent _wait = new AutoResetEvent(false);
        private int i = 0;
        private void Handler()
        {
            i++;
            if(i == 1000000)
                _wait.Set();
        }

        public void Run(IFiber fiber)
        {
            i = 0;
            for (int j = 0; j < 1000000; j++)
            {
                fiber.Enqueue(Handler);
            }
            WaitHandle.WaitAny(new WaitHandle[] { _wait });
        }

        [Benchmark(OperationsPerInvoke = 1000000)]
        public void Pool1()
        {
           Run(_pool1);
        }

        [Benchmark(OperationsPerInvoke = 1000000)]
        public void Pool2()
        {
            Run(_pool2);
        }

        [Benchmark(OperationsPerInvoke = 1000000)]
        public void PoolSpin()
        {
            Run(_spinPool);
        }

        [GlobalSetup]
        public void Setup()
        {
            _pool1 = PoolFiber.StartNew();
            _pool2 = new PoolFiber2();
            _pool2.Start();
            _spinPool  =new SpinLockPoolFiber();
            _spinPool.Start();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pool1.Stop();
            _pool2.Stop();
            _spinPool.Stop();
        }
    }

    public class TrheadFibers
    {
        private IFiber _sleep;
        private IFiber _yield;
        private IFiber _spin;
        private AutoResetEvent _wait = new AutoResetEvent(false);
        private int i = 0;
        private void Handler()
        {
            i++;
            if (i == 1000000)
                _wait.Set();
        }

        public void Run(IFiber fiber)
        {
            i = 0;
            for (int j = 0; j < 1000000; j++)
            {
                fiber.Enqueue(Handler);
            }
            WaitHandle.WaitAny(new WaitHandle[] { _wait });
        }

        [Benchmark]
        public void Sleeping()
        {
            Run(_sleep);
        }

        [Benchmark]
        public void Yielding()
        {
            Run(_yield);
        }

        [Benchmark]
        public void SpinLock()
        {
            Run(_spin);
        }

        [GlobalSetup]
        public void Setup()
        {
            _sleep = new ThreadFiber(new Executor(), new SleepingQueue());
            _sleep.Start();
            _yield = new ThreadFiber(new Executor(), new YieldingQueue());
            _yield.Start();
            _spin = new ThreadFiber(new Executor(), new SpinLockQueue());
            _spin.Start();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _sleep.Stop();
            _yield.Stop();
            _spin.Stop();
        }
    }
}
