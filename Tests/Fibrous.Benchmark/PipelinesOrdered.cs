using System;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Fibrous.Pipelines;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class PipelinesOrdered
    {
        private const int OperationsPerInvoke = 1000000;

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Ordered()
        {
            long index = 0;
            using AutoResetEvent reset = new(false);
            using IStage<int, int> pipe = new Stage<int, int>(x => Enumerable.Range(0, OperationsPerInvoke).ToArray())
                .SelectOrdered(x => x, 4);
            using Fiber fiber = new();
            pipe.Subscribe(fiber, x =>
            {
                index++;
                if (index == OperationsPerInvoke)
                {
                    reset.Set();
                }
            });
            pipe.Publish(0);
            reset.WaitOne(TimeSpan.FromSeconds(10));
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void UnOrdered()
        {
            using AutoResetEvent reset = new(false);
            long index = 0;
            using IStage<int, int> pipe = new Stage<int, int>(x => Enumerable.Range(0, OperationsPerInvoke).ToArray())
                .Select(x => x, 4);
            using Fiber fiber = new();
            pipe.Subscribe(fiber, x =>
            {
                index++;
                if (index == OperationsPerInvoke)
                {
                    reset.Set();
                }
            });
            pipe.Publish(0);
            reset.WaitOne(TimeSpan.FromSeconds(10));
        }
    }
}
