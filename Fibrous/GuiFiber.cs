namespace Fibrous
{
    using System;

    public abstract class GuiFiber : Fiber
    {
        private readonly IExecutionContext _executionContext;

        protected GuiFiber(Executor executor, IExecutionContext executionContext) : base(executor)
        {
            _executionContext = executionContext;
        }

        protected GuiFiber(IExecutionContext executionContext)
        {
            _executionContext = executionContext;
        }

        protected override void InternalEnqueue(Action action)
        {
            _executionContext.Enqueue(() => Executor.Execute(action));
        }
    }
}