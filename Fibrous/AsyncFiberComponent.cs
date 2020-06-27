using System;

namespace Fibrous
{
    public abstract class AsyncFiberComponent : IDisposable
    {
        protected IAsyncFiber Fiber { get; }

        protected AsyncFiberComponent(IFiberFactory factory = null)
        {
            Fiber = factory?.CreateAsync(OnError) ?? new AsyncFiber(OnError);
        }

        protected abstract void OnError(Exception obj);

        public void Dispose()
        {
            Fiber?.Dispose();
        }
    }
}