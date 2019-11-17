using System;
using System.Threading.Tasks;

namespace Fibrous
{
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