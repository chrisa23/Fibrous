using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous.Collections;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class FiberDictionary
    {
        private FiberDictionary<string, string> _dictionary;

        [Benchmark]
        public async Task<KeyValuePair<string, string>[]> Stub()
        {
            return await _dictionary.GetItemsAsync(x => true);
        }

        [GlobalSetup]
        public void Setup()
        {
            _dictionary = new FiberDictionary<string, string>();
            _dictionary.Add("a", "a");
            _dictionary.Add("b", "b");
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _dictionary.Dispose();
        }
    }
}
