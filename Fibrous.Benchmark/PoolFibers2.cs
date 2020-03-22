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
        private IAsyncFiber _asyncLock;
        private IFiber _pool3;
        private IFiber _stub;
        private int _i;
        private IFiber _lock;

        private void Handler(int obj)
        {
            _i++;
            if (_i == OperationsPerInvoke)
                _wait.Set();
        }

        private Task AsyncHandler(int obj)
        {
            _i++;
            if (_i == OperationsPerInvoke)
                _wait.Set();
            return Task.CompletedTask;
        }

        public void Run(IFiber fiber)
        {
            using var sub = _channel.Subscribe(fiber, Handler);

            _i = 0;
            for (var j = 0; j < OperationsPerInvoke; j++) _channel.Publish(0);

            WaitHandle.WaitAny(new WaitHandle[] {_wait});
        }

        public void Run(IAsyncFiber fiber)
        {
            using var sub = _channel.Subscribe(fiber, AsyncHandler);

            _i = 0;
            for (var j = 0; j < OperationsPerInvoke; j++) _channel.Publish(0);

            WaitHandle.WaitAny(new WaitHandle[] {_wait});
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Fiber()
        {
            Run(_pool3);
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Lock()
        {
            Run(_lock);
        }
        
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Async()
        {
            Run(_async);
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncLock()
        {
            Run(_asyncLock);
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Stub()
        {
            Run(_stub);
        }

        [GlobalSetup]
        public void Setup()
        {
            _async = new AsyncFiber();
            _pool3 = new Fiber();
            _stub = new StubFiber();
            _lock = new LockFiber();
            _asyncLock = new AsyncLockFiber();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pool3.Dispose();
            _async.Dispose();
            _stub.Dispose();
            _lock.Dispose();
            _asyncLock.Dispose();
        }
    }
}