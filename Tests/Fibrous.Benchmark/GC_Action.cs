using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class GC_Action
    {
        private readonly IChannel<object> _channel = new Channel<object>();
        private readonly object _msg = new();
        private IAsyncFiber _fiber;

        [Benchmark]
        public void Publish() => _channel.Publish(_msg);

        [GlobalSetup]
        public void Setup()
        {
            _fiber = new AsyncFiber();
            _channel.Subscribe(_fiber, async o => { });
        }

        [GlobalCleanup]
        public void Cleanup() => _fiber.Dispose();
    }
}
