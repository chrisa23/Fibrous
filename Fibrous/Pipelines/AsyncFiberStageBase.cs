using System;
using System.Threading.Tasks;

namespace Fibrous.Pipelines
{
    public abstract class AsyncFiberStageBase<TIn, TOut> : StageBase<TIn, TOut>
    {
        protected AsyncFiberStageBase(Action<Exception> errorCallback = null)
        {
            Fiber = new AsyncFiber(errorCallback);
            Fiber.Subscribe(In, Receive);
        }

        public IAsyncFiber Fiber { get; }

        public override void Dispose() => Fiber?.Dispose();

        protected abstract Task Receive(TIn @in);
    }
}