using System;
using System.Collections.Generic;

namespace Fibrous.Pipelines
{
    internal sealed class OrderedJoin<T> : IDisposable
    {
        private readonly Disposables _disposables = new Disposables();
        private readonly Dictionary<long, T> _output = new Dictionary<long, T>();
        private long _index;

        public OrderedJoin(IChannel<T> output) => Output = output;

        public IChannel<T> Output { get; set; }

        public void Dispose() => _disposables?.Dispose();

        public void Subscribe(ISubscriberPort<Ordered<T>> port) => _disposables.Add(port.Subscribe(Receive));

        private void Receive(Ordered<T> obj)
        {
            lock (_disposables)
            {
                if (obj.Index == _index)
                {
                    _index++;
                    Output.Publish(obj.Item);

                    while (_output.Count > 0 && _output.ContainsKey(_index))
                    {
                        Output.Publish(_output[_index]);
                        _output.Remove(_index);
                        _index++;
                    }

                    return;
                }

                _output[obj.Index] = obj.Item;
            }
        }
    }
}
