namespace Fibrous.Benchmark
{
    using BenchmarkDotNet.Attributes;
    using Fibrous.Fibers;

    [MemoryDiagnoser]
    public class GC_EnqueueLambdaVsMethod
    {
        private IFiber _fiber;
        
        [Benchmark]
        public void Lambda()
        {
            _fiber.Enqueue(() =>{});
        }

        [Benchmark]
        public void Method()
        {
            _fiber.Enqueue(Void);
        }


        public void Void()
        {

        }

        [GlobalSetup]
        public void Setup()
        {
            _fiber = PoolFiber.StartNew();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _fiber.Dispose();
        }
    }
}