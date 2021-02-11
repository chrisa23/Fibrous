using System.Threading;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    public class Increment
    {
        private int _countInt;
        private long _countLong;

        [Benchmark]
        public long InterlockedLong() => Interlocked.Increment(ref _countLong);

        [Benchmark]
        public long InterlockedInt() => Interlocked.Increment(ref _countInt);

        [Benchmark]
        public long RawLong() => ++_countLong;

        [Benchmark]
        public long InterlockedLongMod1() => Interlocked.Increment(ref _countLong) % 1;

        [Benchmark]
        public long InterlockedLongMod2() => Interlocked.Increment(ref _countLong) % 2;

        [Benchmark]
        public long InterlockedLongMod3() => Interlocked.Increment(ref _countLong) % 3;
    }
}
