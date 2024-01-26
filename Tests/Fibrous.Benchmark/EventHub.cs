using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class EventHubBenchmark
    {
        private const int OperationsPerInvoke = 1000000;
        private readonly IEventHub _hub = new EventHub();
        private readonly AutoResetEvent _wait = new(false);


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncFiber()
        {
            using FiberConsumer consumer = new(_hub, _wait);

            for (int j = 0; j < OperationsPerInvoke; j++)
            {
                _hub.Publish(j);
            }

            WaitHandle.WaitAny(new WaitHandle[] {_wait});
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void NoFiber()
        {
            for (int j = 0; j < OperationsPerInvoke; j++)
            {
                _hub.Publish(j);
            }
        }

        public class FiberConsumer : FiberComponent, IHandleAsync<int>
        {
            private readonly AutoResetEvent _reset;

            public FiberConsumer(IEventHub hub, AutoResetEvent reset)
            {
                _reset = reset;
                hub.Subscribe(Fiber, this);
            }

            public Task HandleAsync(int message)
            {
                if (message == OperationsPerInvoke - 1)
                {
                    _reset.Set();
                }

                return Task.CompletedTask;
            }

            protected override void OnError(Exception obj)
            {
            }
        }
    }
}
