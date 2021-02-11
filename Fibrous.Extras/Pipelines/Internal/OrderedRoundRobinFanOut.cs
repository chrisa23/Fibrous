using System.Collections.Generic;
using Fibrous.Pipelines;

namespace Fibrous
{
    internal sealed class OrderedRoundRobinFanOut<T> : Disposables
    {
        private readonly IFiber _fiber = new Fiber();
        private readonly List<IPublisherPort<Ordered<T>>> _stages = new List<IPublisherPort<Ordered<T>>>();
        private long _count;
        private int _index;

        public void AddStage(IPublisherPort<Ordered<T>> stage) => _stages.Add(stage);

        public void SetUpSubscribe(ISubscriberPort<T> port) => port.Subscribe(_fiber, OnReceive);

        private void OnReceive(T obj)
        {
            long i = _count++;
            _stages[_index].Publish(new Ordered<T>(i, obj));
            _index++;
            _index %= _stages.Count;
        }

        public override void Dispose()
        {
            _fiber.Dispose();
            base.Dispose();
        }
    }
}
