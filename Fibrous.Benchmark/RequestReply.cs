using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous;
namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class RequestReply
    {
        private readonly IRequestChannel<int, int> _requestChannel = new RequestChannel<int, int>();
        private IFiber _fiber;
        private IAsyncFiber _asyncFiber;
        private IFiber _fiberReply;

        [Benchmark]
        public async Task<int> FiberReqRep()
        {
            return await _requestChannel.SendRequest(0);
        }
        [Benchmark]
        public async Task<Result<int>> FiberReqRepTimeout()
        {
            return await _requestChannel.SendRequest(0, TimeSpan.FromSeconds(1));
        }

        [GlobalSetup]
        public void Setup()
        {
            _fiber = new Fiber();
            _fiberReply = new Fiber();
            _asyncFiber = new AsyncFiber();
            _requestChannel.SetRequestHandler(_fiberReply, r => r.Reply(1));
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

