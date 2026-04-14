using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Fibrous.Benchmark;

internal class Program
{
    private static void Main(string[] args)
    {
        bool fullRun = args.Contains("--full");
        string[] benchmarkArgs = fullRun ? args.Where(arg => arg != "--full").ToArray() : args;
        IConfig config = fullRun ? DefaultConfig.Instance : DefaultConfig.Instance.AddJob(Job.ShortRun);

        BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly)
            .Run(benchmarkArgs, config);
    }
}
