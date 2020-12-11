using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    public class Decrement
    {
        [Benchmark]
        public int Ref()
        {
            int i = 1;
            YieldingQueue.ApplyWaitMethod(ref i);
            return i;
        }

        [Benchmark]
        public int NoRef()
        {
            int i = 1;
            return YieldingQueue.ApplyWaitMethod2(i);
        }
    }
}
