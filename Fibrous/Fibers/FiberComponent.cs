using System;

namespace Fibrous;

public abstract class FiberComponent : IDisposable
{
    protected FiberComponent(IFiberFactory factory = null) =>
        Fiber = factory?.CreateFiber(OnError) ?? new Fiber(OnError);

    protected IFiber Fiber { get; }

    public void Dispose() => Fiber?.Dispose();

    protected abstract void OnError(Exception obj);
}
