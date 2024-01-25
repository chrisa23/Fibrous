using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fibrous.Pipelines;

namespace Fibrous;

internal sealed class OrderedRoundRobinFanOut<T> : Disposables
{
    private readonly IAsyncFiber                      _fiber = new AsyncFiber(OnException);

    private static void OnException(Exception obj)
    {
        //TODO: what do we do here?
    }

    private readonly List<IPublisherPort<Ordered<T>>> _stages = new();
    private          long                             _count;
    private          int                              _index;

    public void AddStage(IPublisherPort<Ordered<T>> stage) => _stages.Add(stage);

    public void SetUpSubscribe(ISubscriberPort<T> port) => port.Subscribe(_fiber, OnReceive);

    private Task OnReceive(T obj)
    {
        long i = _count++;
        _stages[_index].Publish(new Ordered<T>(i, obj));
        _index++;
        _index %= _stages.Count;
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _fiber.Dispose();
        base.Dispose();
    }
}
