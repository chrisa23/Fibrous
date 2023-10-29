using System;

namespace Fibrous;

public abstract class AsyncFiberComponent : IDisposable
{
    protected AsyncFiberComponent(IFiberFactory factory = null) =>
        Fiber = factory?.CreateAsyncFiber(OnError) ?? new AsyncFiber(OnError);

    protected IAsyncFiber Fiber { get; }

    public void Dispose() => Fiber?.Dispose();

    protected abstract void OnError(Exception obj);
}
