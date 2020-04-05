using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    public class ArrayClearBenchmark
    {
        [Params(16, 32, 64, 128, 1024,2 *1024)] public int N;
        private Action[] _data;
        private readonly Action _noop = () => {};

        [Benchmark]
        public void ArrayClear()
        {
            Array.Clear(_data, 0, N);
        }

        [Benchmark]
        public void Iterate()
        {
            for (int i = 0; i < N; i++)
            {
                _data[i] = _noop;
            }
        }
        [GlobalSetup]
        public void Setup()
        {
            _data = new Action[N];
            for (int i = 0; i < N; i++)
            {
                _data[i] = _noop;
            }
        }

    }
}
