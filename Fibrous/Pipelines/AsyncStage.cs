using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous.Pipelines;

public class AsyncStage<TIn, TOut> : AsyncFiberStageBase<TIn, TOut>
{
    private readonly Func<TIn, Task<TOut>> _f;
    private readonly Func<TIn, Task<IEnumerable<TOut>>> _f2;

    public AsyncStage(Func<TIn, Task<TOut>> f, Action<Exception> errorCallback = null) : base(errorCallback) =>
        _f = f;

    public AsyncStage(Func<TIn, Task<IEnumerable<TOut>>> f, Action<Exception> errorCallback = null) :
        base(errorCallback) => _f2 = f;

    protected override async Task Receive(TIn @in)
    {
        if (_f != null)
        {
            Out.Publish(await _f(@in));
            return;
        }

        foreach (TOut result in await _f2(@in))
        {
            Out.Publish(result);
        }
    }
}
