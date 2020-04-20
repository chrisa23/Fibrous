using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous.Experimental;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class PoolFibers2
    {
        private const int OperationsPerInvoke = 1_000_000;
        private readonly IChannel<int> _channel = new Channel<int>();
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);
        private int _i;

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
            using (fiber)
            {
                using var sub = _channel.Subscribe(fiber, Handler);

                _i = 0;
                for (var j = 0; j < OperationsPerInvoke; j++) _channel.Publish(0);

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        public void Run(IAsyncFiber fiber)
        {
            using (fiber)
            {
                using var sub = _channel.Subscribe(fiber, AsyncHandler);

                _i = 0;
                for (var j = 0; j < OperationsPerInvoke; j++) _channel.Publish(0);

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Fiber()
        {
            Run(new Fiber());
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Async()
        {
            Run(new AsyncFiber());
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Lock()
        {
            Run(new LockFiber());
        }
        
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncLock()
        {
            Run(new LockAsyncFiber());
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Stub()
        {
            Run(new StubFiber());
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncStub()
        {
            Run(new AsyncStubFiber());
        }
    }
}