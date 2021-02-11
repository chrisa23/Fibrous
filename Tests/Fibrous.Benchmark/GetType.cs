using System;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class GetType
    {
        private readonly MyType _msg = new MyType();

        [Benchmark]
        public Type TypeOf() => typeof(MyType);

        [Benchmark]
        public Type GetTypeCall() => _msg.GetType();


        private class MyType
        {
            public int Prop1 { get; set; }
            public string Prop2 { get; set; }

            public void Method()
            {
            }
        }
    }
}
