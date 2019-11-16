using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fibrous.Channels;

namespace Fibrous.Benchmark
{
    
    using System.Threading;
    using BenchmarkDotNet.Attributes;
    using Fibrous.Experimental;
    [MemoryDiagnoser]
    public class FiberContention
    {
        private const int OperationsPerInvoke = 1000000;
        private IFiber _pool1;
        private IFiber _pool2;
        private IFiber _pool3;
        private IFiber _spinPool;
        IAsyncFiber _async;
        private IChannel<object> _channel = new Channel<object>();
        private AutoResetEvent _wait = new AutoResetEvent(false);
        private int i = 0;
        private void Handler(object obj)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
        }
        private Task AsyncHandler(object obj)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
            return Task.CompletedTask;
        }
        public void Run(IFiber fiber)
        {
            using (var sub = _channel.Subscribe(fiber, Handler))
            {
                i = 0;
                Task.Run(Iterate);
                Task.Run(Iterate);

                WaitHandle.WaitAny(new WaitHandle[] { _wait });
            }
        }

        private void Iterate()
        {
            var count = OperationsPerInvoke/2;
            for (int j = 0; j < count; j++)
            {
                _channel.Publish(null);
            }
        }

        public void Run(IAsyncFiber fiber)
        {
            using (var sub = _channel.Subscribe(fiber, AsyncHandler))
            {
                i = 0;
                Task.Run(Iterate);
                Task.Run(Iterate);

                WaitHandle.WaitAny(new WaitHandle[] { _wait });
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Pool1()
        {
            Run(_pool1);
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Pool2()
        {
            Run(_pool2);
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Pool3()
        {
            Run(_pool3);
        }


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void PoolSpin()
        {
            Run(_spinPool);
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
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
            _spinPool = new SpinLockPoolFiber();
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
