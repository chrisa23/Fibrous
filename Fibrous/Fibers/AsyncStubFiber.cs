using System;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Fiber that executes on caller's thread.  For testing and well understood situations.  Use with caution.
    /// </summary>
    public sealed class AsyncStubFiber : AsyncFiberBase
    {
        public AsyncStubFiber(IAsyncExecutor executor = null, IAsyncFiberScheduler scheduler = null)
            : base(executor, scheduler)
        {
        }

        protected override void InternalEnqueue(Func<Task> action)
        {
            Executor.Execute(action).Wait();
        }
    }
}