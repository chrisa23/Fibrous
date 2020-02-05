using System;
using System.Collections.Generic;
using System.Text;

namespace Fibrous
{
    internal sealed class RoundRobinFanOut<T>:Disposables
    {
        private readonly IFiber _fiber = new Fiber();
        private readonly List<IPublisherPort<T>> _stages = new List<IPublisherPort<T>>();
        private int _index;
        public void AddStage(IPublisherPort<T> stage)
        {
            _stages.Add(stage);
        }

        public void SetUpSubscribe(ISubscriberPort<T> port)
        {
            port.Subscribe(_fiber, OnReceive);
        }

        private void OnReceive(T obj)
        {
            _stages[_index].Publish(obj);
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
