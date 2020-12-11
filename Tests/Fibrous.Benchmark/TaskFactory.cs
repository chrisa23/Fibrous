using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class TaskFactory2
    {
        private const int OperationsPerInvoke = 1_000_000;
        private readonly Action _action = () => { };
        private readonly Func<Task> _task = () => Task.CompletedTask;

        private readonly TaskFactory _taskFactory =
            new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);

        public void Action1()
        {
        }

        public Task Task1() => Task.CompletedTask;


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task ActionLambda()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                await _taskFactory.StartNew(_action);
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task ActionMethod()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                await _taskFactory.StartNew(Action1);
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task TaskLambda()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                await _taskFactory.StartNew(_task);
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task TaskMethod()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                await _taskFactory.StartNew(Task1);
            }
        }
    }
}
