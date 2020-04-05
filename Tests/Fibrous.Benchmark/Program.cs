using System.Reflection;
using BenchmarkDotNet.Running;

namespace Fibrous.Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);
        }
    }
}