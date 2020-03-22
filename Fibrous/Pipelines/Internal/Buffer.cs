using System;

namespace Fibrous
{
    //This is interesting for batching,
    //but what happens if buffer not filled...
    internal sealed class Buffer<T> :Disposables
    {
        private readonly int _size;
        private readonly IPublisherPort<T[]> _output;
        private readonly T[] _buffer;
        
        private int _index;
        public Buffer(int size, ISubscriberPort<T> stage1, IPublisherPort<T[]> output)
        {
            _size = size;
            _output = output;
            _buffer = new T[size];
            stage1.Subscribe(OnReceive);
        }

        private void OnReceive(T obj)
        {
            lock (this)
            {
                _buffer[_index++] = obj;
                if (_index > _size - 1)
                {
                    //Find a way to use a pool or something
                    //We can't know when usage is done easily
                    var output = new T[_size];
                    Array.Copy(_buffer, output, _size);
                    _output.Publish(output);
                    
                }
            }
        }
    }
}