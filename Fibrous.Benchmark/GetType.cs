using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class GetType
    {
        private readonly MyType _msg = new MyType();

        [Benchmark]
        public Type TypeOf()
        {
            return typeof(MyType);
        }

        [Benchmark]
        public Type GetTypeCall()
        {
            return _msg.GetType();
        }

 
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
