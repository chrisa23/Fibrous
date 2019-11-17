using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public abstract class GuiFiberBase : FiberBase
    {
        private readonly IExecutionContext _executionContext;

        protected GuiFiberBase(IExecutor executor, IExecutionContext executionContext)
            : base(executor)
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

    public abstract class AsyncGuiFiberBase : AsyncFiberBase
    {
        private readonly IAsyncExecutionContext _executionContext;

        protected AsyncGuiFiberBase(IAsyncExecutor executor, IAsyncExecutionContext executionContext)
            : base(executor)
        {
            _executionContext = executionContext;
        }

        protected AsyncGuiFiberBase(IAsyncExecutionContext executionContext)
        {
            _executionContext = executionContext;
        }

        protected override void InternalEnqueue(Func<Task> action)
        {
            _executionContext.Enqueue(() => Executor.Execute(action));
        }
    }
}