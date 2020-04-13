using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Fibrous.Benchmark
{
    public class Increment
    {
        private long _countLong;
        private int _countInt;

        [Benchmark]
        public long InterlockedLong()
        {
            return Interlocked.Increment(ref _countLong);
        }

        [Benchmark]
        public long InterlockedInt()
        {
            return Interlocked.Increment(ref _countInt);
        }
        [Benchmark]
        public long RawLong()
        {
            return ++_countLong;
        }
        [Benchmark]
        public long InterlockedLongMod1()
        {
            return Interlocked.Increment(ref _countLong) % 1;
        }
        [Benchmark]
        public long InterlockedLongMod2()
        {
            return Interlocked.Increment(ref _countLong) % 2;
        }

        [Benchmark]
        public long InterlockedLongMod3()
        {
            return Interlocked.Increment(ref _countLong) % 3;
        }
    }
}
