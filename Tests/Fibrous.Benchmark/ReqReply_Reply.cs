using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class ReqReply_Reply
    {
        [Benchmark]
        public Task<int> ChannelRequest()
        {
            RequestChannel<int, int>.ChannelRequest obj = new(12);
            obj.Reply(2);
            return obj.Resp.Task;
        }
    }
}
