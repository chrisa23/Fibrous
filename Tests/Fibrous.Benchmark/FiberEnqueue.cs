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

        public void Run(IFiber fiber)
        {
            using AutoResetEvent wait = new(false);
            using (fiber)
            {
                int i = 0;

                void Handler1()
                {
                    i++;
                    if (i == OperationsPerInvoke)
                    {
                        wait.Set();
                    }
                }

                for (int j = 0; j < OperationsPerInvoke; j++)
                {
                    fiber.Enqueue(Handler1);
                }

                WaitHandle.WaitAny(new WaitHandle[] {wait});
            }
        }

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
        public void Run2(IFiber fiber)
        {
            using AutoResetEvent wait = new(false);
            using (fiber)
            {
                int i = 0;
                Action handler = () =>
                {
                    i++;
                    if (i == OperationsPerInvoke)
                    {
                        wait.Set();
                    }
                };
                for (int j = 0; j < OperationsPerInvoke; j++)
                {
                    fiber.Enqueue(handler);
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
        public void Fiber() => Run(new Fiber());


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task TwoFibers()
        {
            Task t1 = Task.Run(() => Run(new Fiber()));
            Task t2 = Task.Run(() => Run(new Fiber()));
            await t1;
            await t2;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task TwoLockFibers()
        {
            Task t1 = Task.Run(() => Run(new LockFiber()));
            Task t2 = Task.Run(() => Run(new LockFiber()));
            await t1;
            await t2;
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
        public void FiberWCache() => Run2(new Fiber());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncWCache() => Run2(new AsyncFiber());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockFiber() => Run(new LockFiber());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockAsync() => Run(new LockAsyncFiber());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockFiberWCache() => Run2(new LockFiber());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockAsyncWCache() => Run2(new LockAsyncFiber());
    }
}
