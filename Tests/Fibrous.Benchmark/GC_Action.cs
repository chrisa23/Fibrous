using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class GC_Action
    {
        private readonly IChannel<object> _channel = new Channel<object>();
        private readonly object _msg = new object();
        private IFiber _fiber;

        [Benchmark]
        public void Publish()
        {
            _channel.Publish(_msg);
        }

        [GlobalSetup]
        public void Setup()
        {
            _fiber = new Fiber();
            _channel.Subscribe(_fiber, o => { });
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _fiber.Dispose();
        }
    }
}