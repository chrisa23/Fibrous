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
        private IRequestChannel<int, int> _requestChannel = new RequestChannel<int, int>();
        private IFiber _fiber;
        private IAsyncFiber _asyncFiber;
        private IFiber _fiberReply;

        [Benchmark]
        public async  Task<int> FiberReqRep()
        {
            return await _requestChannel.SendRequest(0);
        }

     
        [GlobalSetup]
        public void Setup()
        {
            _fiber = Fiber.StartNew();
            _fiberReply = Fiber.StartNew();
            _asyncFiber = AsyncFiber.StartNew();
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

