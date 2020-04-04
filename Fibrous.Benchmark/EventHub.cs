using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class EventHubBenchmark
    {
        private readonly IEventHub _hub = new EventHub();
        private const int OperationsPerInvoke = 1000000;
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public  void Fiber()
        {
            using var consumer = new FiberConsumer(_wait);
            using var sub = _hub.Subscribe(consumer);

            for (var j = 0; j < OperationsPerInvoke; j++) _hub.Publish(j);

            WaitHandle.WaitAny(new WaitHandle[] { _wait });
        }
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncFiber()
        {
            using var consumer = new AsyncFiberConsumer(_wait);
            using var sub = _hub.Subscribe(consumer);

            for (var j = 0; j < OperationsPerInvoke; j++) _hub.Publish(j);

            WaitHandle.WaitAny(new WaitHandle[] { _wait });
        }
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void NoFiber()
        {
            for (var j = 0; j < OperationsPerInvoke; j++) _hub.Publish(j);
        }
        public class FiberConsumer : FiberComponent, IHandle<int>
        {
            private readonly AutoResetEvent _reset;

            public FiberConsumer(AutoResetEvent reset)
            {
                _reset = reset;
            }
            protected override void OnError(Exception obj)
            {

            }

            public void Handle(int message)
            {
                if (message == OperationsPerInvoke - 1)
                    _reset.Set();
            }
        }
        public class AsyncFiberConsumer : AsyncFiberComponent, IHandleAsync<int>
        {
            private readonly AutoResetEvent _reset;

            public AsyncFiberConsumer(AutoResetEvent reset)
            {
                _reset = reset;
            }
            protected override void OnError(Exception obj)
            {

            }

            public Task Handle(int message)
            {
                if (message == OperationsPerInvoke - 1)
                    _reset.Set();

                return Task.CompletedTask;
            }
        }
    }


}
