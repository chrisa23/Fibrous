using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class ReqReply2
    {
        [Benchmark]
        public Task<int> ChannelRequest()
        {
            var obj = new RequestChannel<int, int>.ChannelRequest(12);
            obj.Reply(2);
            return obj.Resp.Task;
        }
    }
}
