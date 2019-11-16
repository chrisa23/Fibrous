using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    public class Swap
    {
        private List<Action> list1 = new List<Action>();
        private List<Action> list2 = new List<Action>();

        [Benchmark]
        public List<Action> Swap1()
        {
            Lists.Swap(ref list1, ref list2);
            return list1;
        }

        [Benchmark]
        public List<Action> Interlocks()
        {
            Lists.Swap2(ref list1, ref list2);
            return list2;
        }
    }
}