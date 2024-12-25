using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Example1.Pipelines.Internal;

//Should this use a stub fiber?
//this should probably not start immediately...
internal class Batch<T> : AsyncFiberStageBase<T, T[]>
{
    private readonly List<T> _batch = new();
    private readonly TimeSpan _time;
    private IDisposable _sub;

    public Batch(TimeSpan time, Action<Exception> errorCallback) : base(errorCallback) => _time = time;

    private void Flush()
    {
        if (_batch.Count > 0)
        {
            T[] toSend = _batch.ToArray();
            _batch.Clear();
            Out.Publish(toSend);
        }
    }

    protected override Task Receive(T @in)
    {
        if (_sub == null)
        {
            _sub = Fiber.Schedule(Flush, _time, _time);
        }

        _batch.Add(@in);
        return Task.CompletedTask;
    }
}
