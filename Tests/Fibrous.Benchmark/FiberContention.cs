using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class FiberContention
    {
        private const int OperationsPerInvoke = 1000000;
        private readonly IChannel<object> _channel = new Channel<object>();
        private readonly AutoResetEvent _wait = new(false);
        private int i;

        private void Handler(object obj)
        {
            i++;
            if (i == OperationsPerInvoke)
            {
                _wait.Set();
            }
        }

        private Task AsyncHandler(object obj)
        {
            i++;
            if (i == OperationsPerInvoke)
            {
                _wait.Set();
            }

            return Task.CompletedTask;
        }

        public void Run(IFiber fiber)
        {
            using (fiber)
            {
                using IDisposable sub = _channel.Subscribe(fiber, Handler);
                i = 0;
                Task.Run(Iterate);
                Task.Run(Iterate);

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        private void Iterate()
        {
            int count = OperationsPerInvoke / 2;
            for (int j = 0; j < count; j++)
            {
                _channel.Publish(null);
            }
        }

        public void Run(IAsyncFiber fiber)
        {
            using (fiber)
            {
                using IDisposable sub = _channel.Subscribe(fiber, AsyncHandler);
                i = 0;
                Task.Run(Iterate);
                Task.Run(Iterate);

                WaitHandle.WaitAny(new WaitHandle[] {_wait});
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Fiber2() => Run(new Fiber2());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Pool1() => Run(PoolFiber_OLD.StartNew());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Fiber() => Run(new Fiber());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockFiber() => Run(new LockFiber());


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Async() => Run(new AsyncFiber());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncLock() => Run(new LockAsyncFiber());
    }
}
