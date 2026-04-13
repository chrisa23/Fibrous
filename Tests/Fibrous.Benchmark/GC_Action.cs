using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class GC_Action
    {
        private readonly Channel<object> _channel = new();
        private readonly object _msg = new();
        private IFiber _fiber;

        [Benchmark]
        public void Publish() => _channel.Publish(_msg);

        [GlobalSetup]
        public void Setup()
        {
            _fiber = new Fiber();
            _channel.Subscribe(_fiber, async o => { });
        }

        [GlobalCleanup]
        public void Cleanup() => _fiber.Dispose();
    }
}
