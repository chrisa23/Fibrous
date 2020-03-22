using System;

namespace Fibrous.Pipelines
{
    public abstract class FiberStageBase<TIn, TOut> : StageBase<TIn, TOut>
    {
        protected FiberStageBase(Action<Exception> errorCallback = null)
        {
            Fiber = new Fiber(errorCallback);
            Fiber.Subscribe(In, Receive);
        }

        protected IFiber Fiber { get; }
        
        public override void Dispose() => Fiber.Dispose();

        protected abstract void Receive(TIn @in);
    }
}