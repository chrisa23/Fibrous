using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class CastVsAs
    {
        private readonly object channel = new Channel<int>();


        [Benchmark]
        public IChannel<int> Cast()
        {
            return (IChannel<int>) channel;
        }

        [Benchmark]
        public IChannel<int> As()
        {
            return channel as IChannel<int>;
        }
    }
}
