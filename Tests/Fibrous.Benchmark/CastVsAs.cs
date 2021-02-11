using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class CastVsAs
    {
        private readonly object _channel = new Channel<int>();


        [Benchmark]
        public IChannel<int> Cast() => (IChannel<int>)_channel;

        [Benchmark]
        public IChannel<int> As() => _channel as IChannel<int>;
    }
}
