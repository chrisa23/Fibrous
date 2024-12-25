using System.Collections.Generic;
using Fibrous;

namespace Example1.Pipelines.Internal;

internal sealed class RoundRobinFanOut<T> : Disposables
{
    private readonly List<IPublisherPort<T>> _stages = new();
    private int _index;

    public RoundRobinFanOut(ISubscriberPort<T> port) => port.Subscribe(OnReceive);

    public void AddStage(IPublisherPort<T> stage) => _stages.Add(stage);

    private void OnReceive(T obj)
    {
        lock (this)
        {
            _stages[_index].Publish(obj);
            _index++;
            _index %= _stages.Count;
        }
    }
}
