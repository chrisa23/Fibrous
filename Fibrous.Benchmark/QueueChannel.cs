using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous.Experimental;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class QueueChannel
    {
        private const int OperationsPerInvoke = 1000000;
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);
        private readonly IChannel<string> _queue = new QueueChannel<string>();
        private IAsyncFiber _async;
        private IFiber _pool;
        private IFiber _pool2; 
        private IFiber _pool3;
        
        private int i;

        private void Handler(string s)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
        }

        private Task AsyncHandler(string s)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
            return Task.CompletedTask;
        }

        public void Run(IFiber fiber)
        {

            using var sub = _queue.Subscribe(fiber, Handler);
            i = 0;
            for (var j = 0; j < OperationsPerInvoke; j++) _queue.Publish("0");
            WaitHandle.WaitAny(new WaitHandle[] {_wait});
        }

        public void Run(IAsyncFiber fiber)
        {
            using var sub = _queue.Subscribe(fiber, AsyncHandler);
            i = 0;
            for (var j = 0; j < OperationsPerInvoke; j++) _queue.Publish("0");
            WaitHandle.WaitAny(new WaitHandle[] {_wait});
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke, Baseline = true)]
        public void Pool()
        {
            Run(_pool);
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Async()
        {
            Run(_async);
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void TwoMixed()
        {
            using var sub = _queue.Subscribe(_pool, Handler);
            using var sub2 = _queue.Subscribe(_async, AsyncHandler);
            i = 0;
            for (var j = 0; j < OperationsPerInvoke; j++) _queue.Publish("0");
            WaitHandle.WaitAny(new WaitHandle[] { _wait });
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Three()
        {
            using var sub = _queue.Subscribe(_pool, Handler);
            using var sub2 = _queue.Subscribe(_pool2, Handler);
            using var sub3 = _queue.Subscribe(_pool3, Handler);
            i = 0;
            for (var j = 0; j < OperationsPerInvoke; j++) _queue.Publish("0");
            WaitHandle.WaitAny(new WaitHandle[] { _wait });
        }



        [GlobalSetup]
        public void Setup()
        {
            _async = new AsyncFiber();
            _pool = new Fiber();
            _pool2 = new Fiber();
            _pool3= new Fiber();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pool.Dispose();
            _async.Dispose();
        }
    }
}