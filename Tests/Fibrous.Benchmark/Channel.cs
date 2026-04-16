using System;
using System.Threading;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class Channel
    {
        private const int OperationsPerInvoke = 1000000;
        private readonly Event<int> _event = new();
        private readonly AutoResetEvent _wait = new(false);
        private int i;

        private void Handler(int obj)
        {
            i++;
            if (i == 1000000)
            {
                _wait.Set();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void NoFiber()
        {
            using IDisposable sub = _event.Subscribe(Handler);
            i = 0;
            for (int j = 0; j < 1000000; j++)
            {
                _event.Publish(0);
            }

            WaitHandle.WaitAny(new WaitHandle[] {_wait});
        }
    }
}
