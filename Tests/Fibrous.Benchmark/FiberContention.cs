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
        private readonly Channel<object> _channel = new();
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


        private void Iterate()
        {
            int count = OperationsPerInvoke / 2;
            for (int j = 0; j < count; j++)
            {
                _channel.Publish(null);
            }
        }

        public void Run(IFiber fiber)
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
        public void Async() => Run(new Fiber());
    }
}
