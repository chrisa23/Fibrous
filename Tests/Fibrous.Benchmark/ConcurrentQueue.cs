using System.Collections.Concurrent;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class ConcurrentQueue
    {
        private const int OperationsPerInvoke = 1000000;
        private readonly ConcurrentQueue<int> _queue = new ConcurrentQueue<int>();

        [IterationSetup]
        public void Setup()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                _queue.Enqueue(i);
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public int TryTake()
        {
            while (_queue.TryDequeue(out int m))
            {
            }

            return 0;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task TryTake2()
        {
            Task a1 = Task.Run(() =>
            {
                while (_queue.TryDequeue(out int m))
                {
                }
            });
            Task a2 = Task.Run(() =>
            {
                while (_queue.TryDequeue(out int m))
                {
                }
            });
            await a1;
            await a2;
        }
    }
}
