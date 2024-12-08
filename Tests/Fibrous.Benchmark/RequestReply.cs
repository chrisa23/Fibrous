using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class RequestReply
    {
        private readonly IRequestChannel<int, int> _asyncRequestChannel = new RequestChannel<int, int>();

        private readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
        private readonly IRequestChannel<int, int> _requestChannel = new RequestChannel<int, int>();
        private IFiber _fiber;


        [Benchmark]
        public async Task<int> FiberReqRep() => await _requestChannel.SendRequestAsync(0);

        [Benchmark]
        public async Task<Reply<int>> FiberReqRepTimeout() =>
            await _requestChannel.SendRequestAsync(0, _oneSecond);

        [Benchmark]
        public async Task<int> AsyncFiberReqRep() => await _asyncRequestChannel.SendRequestAsync(0);

        [Benchmark]
        public async Task<Reply<int>> AsyncFiberReqRepTimeout() =>
            await _asyncRequestChannel.SendRequestAsync(0, _oneSecond);


        [GlobalSetup]
        public void Setup()
        {

            _fiber = new Fiber();
            _requestChannel.SetRequestHandler(_fiber, async r => r.Reply(1));
            _asyncRequestChannel.SetRequestHandler(_fiber, async r => r.Reply(1));
        }

        [GlobalCleanup]
        public void Cleanup()
        {

            _fiber.Dispose();
        }
    }
}
