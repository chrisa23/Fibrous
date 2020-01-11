using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous.Experimental;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class FiberContention
    {
        private const int OperationsPerInvoke = 1000000;
        private readonly IChannel<object> _channel = new Channel<object>();
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);
        private IAsyncFiber _async;
        private IFiber _pool1;
        private IFiber _pool2;
        private IFiber _pool3;
        private IFiber _spinPool;
        private int i;

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

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        private void Iterate()
        {
            var count = OperationsPerInvoke / 2;
            for (var j = 0; j < count; j++) _channel.Publish(null);
        }

        public void Run(IAsyncFiber fiber)
        {
            using (var sub = _channel.Subscribe(fiber, AsyncHandler))
            {
                i = 0;
                Task.Run(Iterate);
                Task.Run(Iterate);

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Pool1()
        {
            Run(_pool1);
        }

        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void Pool2()
        //{
        //    Run(_pool2);
        //}

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Pool3()
        {
            Run(_pool3);
        }


        //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        //public void PoolSpin()
        //{
        //    Run(_spinPool);
        //}

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
            _spinPool = new SpinLockPoolFiber();
            _async = new AsyncFiber();
            _pool3 = new Fiber();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pool1.Dispose();
            _pool2.Dispose();
            _spinPool.Dispose();
            _pool3.Dispose();
            _async.Dispose();
        }
    }
}