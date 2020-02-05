using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous.Experimental;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class PoolFibers2
    {
        private const int OperationsPerInvoke = 1000000;
        private readonly IChannel<int> _channel = new Channel<int>();
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);
        private IAsyncFiber _async;
        private IFiber _pool3;
        private IFiber _stub;
        private int i;

        private void Handler(int obj)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
        }

        private Task AsyncHandler(int obj)
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
                for (var j = 0; j < OperationsPerInvoke; j++) _channel.Publish(0);

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        public void Run(IAsyncFiber fiber)
        {
            using (var sub = _channel.Subscribe(fiber, AsyncHandler))
            {
                i = 0;
                for (var j = 0; j < OperationsPerInvoke; j++) _channel.Publish(0);

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        //[Benchmark(OperationsPerInvoke = 1000000, Baseline = true)]
        //public void Pool1Old()
        //{
        //    Run(_pool1);
        //}

        //[Benchmark(OperationsPerInvoke = 1000000)]
        //public void Pool2()
        //{
        //    Run(_pool2);
        //}

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Fiber()
        {
            Run(_pool3);
        }


        //[Benchmark(OperationsPerInvoke = 1000000)]
        //public void PoolSpin()
        //{
        //    Run(_spinPool);
        //}

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Async()
        {
            Run(_async);
        }


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Stub()
        {
            Run(_stub);
        }

        [GlobalSetup]
        public void Setup()
        {
            //_pool1 = new PoolFiber_OLD();
            //_pool1.Start();
            //_pool2 = new PoolFiber2();
            //_pool2.Start();
            //_spinPool = new SpinLockPoolFiber();
            //_spinPool.Start();
            _async = new AsyncFiber();
            _pool3 = new Fiber();
            _stub = new StubFiber();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            //_pool1.Stop();
            //_pool2.Stop();
            //_spinPool.Stop();
            _pool3.Dispose();
            _async.Dispose();
            _stub.Dispose();
        }
    }
}