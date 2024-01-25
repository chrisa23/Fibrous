using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous.Benchmark.Implementations;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class FiberEnqueue
    {
        private const int OperationsPerInvoke = 1_000_000;

        public void Run(IAsyncFiber fiber)
        {
            using AutoResetEvent wait = new(false);
            using (fiber)
            {
                int i = 0;

                Task AsyncHandler()
                {
                    i++;
                    if (i == OperationsPerInvoke)
                    {
                        wait.Set();
                    }

                    return Task.CompletedTask;
                }

                for (int j = 0; j < OperationsPerInvoke; j++)
                {
                    fiber.Enqueue(AsyncHandler);
                }

                WaitHandle.WaitAny(new WaitHandle[] {wait});
            }
        }

        public void Run(IValueAsyncFiber fiber)
        {
            using AutoResetEvent wait = new(false);
            using (fiber)
            {
                int i = 0;

                ValueTask AsyncHandler()
                {
                    i++;
                    if (i == OperationsPerInvoke)
                    {
                        wait.Set();
                    }

                    return new ValueTask();
                }

                for (int j = 0; j < OperationsPerInvoke; j++)
                {
                    fiber.Enqueue(AsyncHandler);
                }

                WaitHandle.WaitAny(new WaitHandle[] {wait});
            }
        }



        //0 allocations when caching the handler to Action
        public void Run2(IAsyncFiber fiber)
        {
            using AutoResetEvent wait = new(false);
            using (fiber)
            {
                int i = 0;
                Func<Task> handler = () =>
                {
                    i++;
                    if (i == OperationsPerInvoke)
                    {
                        wait.Set();
                    }

                    return Task.CompletedTask;
                };
                for (int j = 0; j < OperationsPerInvoke; j++)
                {
                    fiber.Enqueue(handler);
                }

                WaitHandle.WaitAny(new WaitHandle[] {wait});
            }
        }


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task TwoAsyncFibers()
        {
            Task t1 = Task.Run(() => Run(new AsyncFiber()));
            Task t2 = Task.Run(() => Run(new AsyncFiber()));
            await t1;
            await t2;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task TwoLockAsyncFibers()
        {
            Task t1 = Task.Run(() => Run(new LockAsyncFiber()));
            Task t2 = Task.Run(() => Run(new LockAsyncFiber()));
            await t1;
            await t2;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Async() => Run(new AsyncFiber());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void ValueAsync() => Run(new ValueAsyncFiber());


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncWCache() => Run2(new AsyncFiber());


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockAsync() => Run(new LockAsyncFiber());


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockAsyncWCache() => Run2(new LockAsyncFiber());
    }
}
