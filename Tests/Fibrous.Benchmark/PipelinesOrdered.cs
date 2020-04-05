using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Fibrous.Agents;
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
            using var reset = new AutoResetEvent(false);
            using var pipe = new Stage<int, int>(x => Enumerable.Range(0, OperationsPerInvoke).ToArray())
                .SelectOrdered(x => x, 4);
            using var fiber = new Fiber();
            pipe.Subscribe(fiber, x =>
            {
                index++;
                if (index == OperationsPerInvoke)
                    reset.Set();
            });
            pipe.Publish(0);
            reset.WaitOne(TimeSpan.FromSeconds(10));
          
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void UnOrdered()
        {
            using var reset = new AutoResetEvent(false);
            long index = 0;
            using var pipe = new Stage<int, int>(x => Enumerable.Range(0, OperationsPerInvoke).ToArray())
                .Select(x => x, 4);
            using var fiber = new Fiber();
            pipe.Subscribe(fiber, x =>
            {
                index++;
                if (index == OperationsPerInvoke)
                    reset.Set();
            });
            pipe.Publish(0);
            reset.WaitOne(TimeSpan.FromSeconds(10));

        }
    }
}
