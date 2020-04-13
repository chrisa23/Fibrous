using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class ConcurrentQueue
    {
        private ConcurrentQueue<int> _queue = new ConcurrentQueue<int>();
        private const int OperationsPerInvoke = 1000000;
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
            while(_queue.TryDequeue(out var m))
            {

            }
            return 0;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task TryTake2()
        {
            var a1 = Task.Run(() =>
            {
                while (_queue.TryDequeue(out var m))
                {
                }
            });
            var a2 = Task.Run(() =>
            {
                while (_queue.TryDequeue(out var m)) { }
            });
            await a1;
            await a2;
        }
    }
}
