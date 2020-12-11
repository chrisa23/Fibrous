using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Fibrous.Pipelines;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class Pipelines
    {
        private const int OperationsPerInvoke = 100000;

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Simple()
        {
            long index = 0;
            using AutoResetEvent reset = new AutoResetEvent(false);
            using IStage<int, int> pipe = new Stage<int, int>(x => x)
                .Select(x => x)
                .Select(x => x)
                .Select(x => x);

            pipe.Subscribe(x =>
            {
                index++;
                if (index == OperationsPerInvoke)
                {
                    reset.Set();
                }
            });
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                pipe.Publish(i);
            }

            reset.WaitOne(TimeSpan.FromSeconds(10));
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Complex1()
        {
            long index = 0;
            using AutoResetEvent reset = new AutoResetEvent(false);
            using IStage<int, int> pipe = new Stage<int, int>(x => x)
                .SelectOrdered(x => x, 4)
                .Where(x => x % 2 == 0)
                .Select(x => x);

            pipe.Subscribe(x =>
            {
                index++;
                if (index == OperationsPerInvoke / 2)
                {
                    reset.Set();
                }
            });
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                pipe.Publish(i);
            }

            reset.WaitOne(TimeSpan.FromSeconds(10));
        }
    }
}
