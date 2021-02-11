using System;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    public class ArrayClearBenchmark
    {
        private readonly Action _noop = () => { };
        private Action[] _data;

        [Params(16, 32, 64, 128, 1024, 2 * 1024)]
        public int N;

        [Benchmark]
        public void ArrayClear() => Array.Clear(_data, 0, N);

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
