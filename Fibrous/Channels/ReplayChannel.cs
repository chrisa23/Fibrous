namespace Fibrous.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Simple snapshot mechanism.  Replay all old messages on subscribe.  
    /// Can cause memory leaks if not handled with care
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ReplayChannel<T> : IChannel<T>
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

        public void Publish(T msg)
        {
            lock (_lock)
            {
                _list.Add(msg);
                _updateChannel.Publish(msg);
                Monitor.PulseAll(_lock);
            }
        }

        public void Clear()
        {
            _list.Clear();
        }
    }
}