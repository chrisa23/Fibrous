using System;

namespace Fibrous
{
    public abstract class AsyncConcurrentComponent : IHaveAsyncFiber
    {
        public IAsyncFiber Fiber { get; }

        protected AsyncConcurrentComponent()
        {
            Fiber = new AsyncFiber(OnError);
        }
        protected AsyncConcurrentComponent(IFiberFactory factory)
        {
            Fiber =factory.CreateAsync(OnError);
        }

        protected abstract void OnError(Exception obj);

        public void Dispose()
        {
            Fiber?.Dispose();
        }
    }
}