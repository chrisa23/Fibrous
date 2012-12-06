namespace Fibrous.Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Channels;

    public sealed class ListChannel<T> : IChannel<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _lock = new object();
        private readonly IChannel<T> _updateChannel = new Channel<T>();

        public IDisposable Subscribe(IFiber fiber, Action<T> handler)
        {
            lock (_lock)
            {
                IDisposable disposable = _updateChannel.Subscribe(fiber, handler);
                int length = _list.Count;
                for (int index = 0; index < length; index++)
                {
                    T item = _list[index];
                    fiber.Enqueue(() => handler(item));
                }
                return disposable;
            }
        }

        public bool Publish(T msg)
        {
            lock (_lock)
            {
                _list.Add(msg);
                bool publish = _updateChannel.Publish(msg);
                Monitor.PulseAll(_lock);
                return publish;
            }
        }
    }
}