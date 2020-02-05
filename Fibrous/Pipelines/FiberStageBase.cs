using System;

namespace Fibrous.Pipelines
{
    public abstract class FiberStageBase<TIn, TOut> : StageBase<TIn, TOut>
    {
        protected FiberStageBase(Action<Exception> errorCallback = null)
        {
            IExecutor executor = errorCallback == null
                ? (IExecutor)new Executor()
                : new ExceptionHandlingExecutor(errorCallback);
            Fiber = new Fiber(executor);
            Fiber.Subscribe(In, Receive);
        }

        protected IFiber Fiber { get; }
        
        public override void Dispose()
        {
            Fiber.Dispose();
        }

        protected abstract void Receive(TIn @in);
    }
}