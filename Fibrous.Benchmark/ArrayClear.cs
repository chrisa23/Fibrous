using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    public class ArrayClearBenchmark
    {
        [Params(16, 32, 64, 128, 1024,2 *1024)] public int N;
        private Action[] data;
        private readonly Action noop = () => {};

        [Benchmark]
        public void ArrayClear()
        {
            Array.Clear(data, 0, N);
        }

        [Benchmark]
        public void Iterate()
        {
            for (int i = 0; i < N; i++)
            {
                data[i] = noop;
            }
        }
        [GlobalSetup]
        public void Setup()
        {
            data = new Action[N];
            for (int i = 0; i < N; i++)
            {
                data[i] = noop;
            }
        }

    }
}
