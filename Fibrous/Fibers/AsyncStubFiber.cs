using System;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Fiber that executes on caller's thread.  For testing and well understood situations.  Use with caution.
    /// </summary>
    public sealed class AsyncStubFiber : AsyncFiberBase
    {
        public AsyncStubFiber(IAsyncExecutor executor, IAsyncFiberScheduler scheduler)
            : base(executor, scheduler)
        {
        }

        public AsyncStubFiber(IAsyncExecutor executor) : base(executor)
        {
        }

        public AsyncStubFiber()
        {
        }


        public static IAsyncFiber StartNew()
        {
            var stub = new AsyncStubFiber();
            stub.Start();
            return stub;
        }

        protected override void InternalEnqueue(Func<Task> action)
        {
            Executor.Execute(action).Wait();
        }
    }
}