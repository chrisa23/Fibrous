using System;

namespace Fibrous.Pipelines
{
    public abstract class StageStubFiberBase<TIn, TOut> : StageBase<TIn, TOut>, IHaveFiber
    {
        protected StageStubFiberBase(Action<Exception> errorCallback = null)
        {
            IExecutor executor = errorCallback == null
                ? (IExecutor)new Executor()
                : new ExceptionHandlingExecutor(errorCallback);
            Fiber = new StubFiber(executor);
            Fiber.Subscribe(In, Receive);
        }

        public IFiber Fiber { get; }

        public override void Dispose()
        {
            Fiber?.Dispose();
        }

        protected abstract void Receive(TIn @in);
    }
}