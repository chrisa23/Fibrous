using System;

namespace Fibrous
{
    public abstract class AsyncFiberComponent : IHaveAsyncFiber
    {
        public IAsyncFiber Fiber { get; }

        protected AsyncFiberComponent()
        {
            Fiber = new AsyncFiber(OnError);
        }
        protected AsyncFiberComponent(IFiberFactory factory)
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