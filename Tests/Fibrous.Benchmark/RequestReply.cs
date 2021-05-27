using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class RequestReply
    {
        private readonly IRequestChannel<int, int> _requestChannel = new RequestChannel<int, int>();
        private readonly IRequestChannel<int, int> _asyncRequestChannel = new RequestChannel<int, int>();
        private IAsyncFiber _asyncFiber;
        private IFiber _fiber;
        private IFiber _fiberReply;

        private readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);

        [Benchmark]
        public async Task<int> FiberReqRep() => await _requestChannel.SendRequestAsync(0);

        [Benchmark]
        public async Task<Result<int>> FiberReqRepTimeout() =>
            await _requestChannel.SendRequestAsync(0, oneSecond);

        [Benchmark]
        public async Task<int> AsyncFiberReqRep() => await _asyncRequestChannel.SendRequestAsync(0);

        [Benchmark]
        public async Task<Result<int>> AsyncFiberReqRepTimeout() =>
            await _asyncRequestChannel.SendRequestAsync(0, oneSecond);


        [GlobalSetup]
        public void Setup()
        {
            _fiber = new Fiber();
            _fiberReply = new Fiber();
            _asyncFiber = new AsyncFiber();
            _requestChannel.SetRequestHandler(_fiberReply, r => r.Reply(1));
            _asyncRequestChannel.SetRequestHandler(_asyncFiber, async r => r.Reply(1));
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _fiber.Dispose();
            _fiberReply.Dispose();
            _asyncFiber.Dispose();
        }
    }
}
