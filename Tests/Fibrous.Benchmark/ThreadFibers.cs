using System.Threading;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    public class ThreadFibers
    {
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);
        private FiberBase_old _sleep;
        private FiberBase_old _spin;
        private FiberBase_old _yield;
        private int i;

        private void Handler()
        {
            i++;
            if (i == 1000000)
                _wait.Set();
        }

        public void Run(IFiber fiber)
        {
            i = 0;
            for (var j = 0; j < 1000000; j++) fiber.Enqueue(Handler);
            WaitHandle.WaitAny(new WaitHandle[] {_wait});
        }

        [Benchmark(OperationsPerInvoke = 1000000)]
        public void Sleeping()
        {
            Run(_sleep);
        }

        [Benchmark(OperationsPerInvoke = 1000000)]
        public void Yielding()
        {
            Run(_yield);
        }

        [Benchmark(OperationsPerInvoke = 1000000)]
        public void SpinLock()
        {
            Run(_spin);
        }

        [GlobalSetup]
        public void Setup()
        {
            _sleep = new ThreadFiber(new Executor(), new SleepingQueue());
            _sleep.Start();
            _yield = new ThreadFiber(new Executor(), new YieldingQueue());
            _yield.Start();
            _spin = new ThreadFiber(new Executor(), new SpinLockQueue());
            _spin.Start();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _sleep.Stop();
            _yield.Stop();
            _spin.Stop();
        }
    }
}