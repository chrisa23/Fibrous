using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fibrous.Channels;

namespace Fibrous.Benchmark
{
    using System.Threading;
    using BenchmarkDotNet.Attributes;
    using Fibrous.Experimental;
    [MemoryDiagnoser]
    public class PoolFibers
    {
        private IFiber _pool1;
        private IFiber _pool2;
        private IFiber _pool3;
        private IFiber _spinPool;
        IAsyncFiber _async;

        private AutoResetEvent _wait = new AutoResetEvent(false);
        private int i = 0;
        private void Handler()
        {
            i++;
            if(i == 1000000)
                _wait.Set();
        }
        private Task AsyncHandler()
        {
            i++;
            if (i == 1000000)
                _wait.Set();
            return Task.CompletedTask;
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

        public void Run(IAsyncFiber fiber)
        {
            i = 0;
            for (int j = 0; j < 1000000; j++)
            {
                fiber.Enqueue(AsyncHandler);
            }
            WaitHandle.WaitAny(new WaitHandle[] { _wait });
        }

        [Benchmark(OperationsPerInvoke = 1000000, Baseline =true)]
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
        public void Pool3()
        {
            Run(_pool3);
        }


        [Benchmark(OperationsPerInvoke = 1000000)]
        public void PoolSpin()
        {
            Run(_spinPool);
        }

        [Benchmark(OperationsPerInvoke = 1000000)]
        public void Async()
        {
            Run(_async);
        }

        [GlobalSetup]
        public void Setup()
        {
            _pool1 = PoolFiber_OLD.StartNew();
            _pool2 = new PoolFiber2();
            _pool2.Start();
            _spinPool  =new SpinLockPoolFiber();
            _spinPool.Start();
            _async = AsyncFiber.StartNew();
            _pool3 = PoolFiber.StartNew();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pool1.Stop();
            _pool2.Stop();
            _spinPool.Stop();
            _pool3.Stop();
            _async.Stop();
        }
    }
}
