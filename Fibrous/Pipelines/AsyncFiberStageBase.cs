using System;
using System.Threading.Tasks;

namespace Fibrous.Pipelines;

public abstract class AsyncFiberStageBase<TIn, TOut> : StageBase<TIn, TOut>
{
    protected AsyncFiberStageBase(Action<Exception> errorCallback = null)
    {
        Fiber = new Fiber(errorCallback);
        Fiber.Subscribe(In, Receive);
    }

    public IFiber Fiber { get; }

    public override void Dispose() => Fiber?.Dispose();

    protected abstract Task Receive(TIn @in);
}
