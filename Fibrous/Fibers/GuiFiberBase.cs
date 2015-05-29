namespace Fibrous
{
    using System;

    public abstract class GuiFiberBase : FiberBase
    {
        private readonly IExecutionContext _executionContext;

        protected GuiFiberBase(Executor executor, IExecutionContext executionContext) : base(executor)
        {
            _executionContext = executionContext;
        }

        protected GuiFiberBase(IExecutionContext executionContext)
        {
            _executionContext = executionContext;
        }

        protected override void InternalEnqueue(Action action)
        {
            _executionContext.Enqueue(() => Executor.Execute(action));
        }
    }
}