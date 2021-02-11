using System;

namespace Fibrous
{
    /// <summary>
    ///     Fiber that executes on caller's thread.  For testing and well understood situations.  Use with caution.
    /// </summary>
    public sealed class StubFiber : FiberBase
    {
        public StubFiber(IExecutor executor = null, IFiberScheduler scheduler = null)
            : base(executor, scheduler)
        {
        }

        public StubFiber(Action<Exception> errorCallback, IFiberScheduler scheduler = null)
            : this(new ExceptionHandlingExecutor(errorCallback), scheduler)
        {
        }

        protected override void InternalEnqueue(Action action) =>
            //There is no lock here to force sequentiality, since that will cause deadlocks in some
            //situations.  Stub fibers are not thread safe.  
            Executor.Execute(action);
    }
}
