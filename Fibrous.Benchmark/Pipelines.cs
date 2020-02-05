using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            using var reset = new AutoResetEvent(false);
            using var pipe = new Stage<int, int>(x => x)
                .Select(x => x)
                .Select(x => x)
                .Select(x => x);
            
            pipe.Subscribe(x =>
            {
                index++;
                if (index == OperationsPerInvoke)
                    reset.Set();
            });
            for (int i = 0; i <  OperationsPerInvoke; i++)
            {
                pipe.Publish(i);
            }
            reset.WaitOne(TimeSpan.FromSeconds(10));

        }
    }

}

