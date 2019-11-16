using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous.Experimental;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class PoolFibers2
    {
        private IAsyncFiber _async;
        private readonly IChannel<object> _channel = new Channel<object>();
        private IFiber _pool1;
        private IFiber _pool2;
        private IFiber _pool3;
        private IFiber _spinPool;
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);
        private int i;

        private void Handler(object obj)
        {
            i++;
            if (i == 1000000)
                _wait.Set();
        }

        private Task AsyncHandler(object obj)
        {
            i++;
            if (i == 1000000)
                _wait.Set();
            return Task.CompletedTask;
        }

        public void Run(IFiber fiber)
        {
            using (var sub = _channel.Subscribe(fiber, Handler))
            {
                i = 0;
                for (var j = 0; j < 1000000; j++) _channel.Publish(null);

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        public void Run(IAsyncFiber fiber)
        {
            using (var sub = _channel.Subscribe(fiber, AsyncHandler))
            {
                i = 0;
                for (var j = 0; j < 1000000; j++) _channel.Publish(null);

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        [Benchmark(OperationsPerInvoke = 1000000, Baseline = true)]
        public void Pool1Old()
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