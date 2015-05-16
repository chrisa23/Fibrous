namespace Fibrous.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    //simplest snapshot mechanism.  Replay all old messages on subscribe...
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

        public bool Publish(T msg)
        {
            lock (_lock)
            {
                _list.Add(msg);
                bool listening = _updateChannel.Publish(msg);
                Monitor.PulseAll(_lock);
                return listening;
            }
        }

        public void Clear()
        {
            _list.Clear();
        }
    }

    public sealed class KeyedReplayChannel<TKey, T> : IChannel<T>
    {
        private readonly Func<T, TKey> _keyMaker;
        private readonly Dictionary<TKey, T> _list = new Dictionary<TKey, T>();
        private readonly object _lock = new object();
        private readonly IChannel<T> _updateChannel = new Channel<T>();

        public KeyedReplayChannel(Func<T, TKey> keyMaker)
        {
            _keyMaker = keyMaker;
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> handler)
        {
            lock (_lock)
            {
                IDisposable disposable = _updateChannel.Subscribe(fiber, handler);
                foreach (T item in _list.Values)
                    handler(item);
                return disposable;
            }
        }

        public bool Publish(T msg)
        {
            lock (_lock)
            {
                _list[_keyMaker(msg)] = msg;
                bool listening = _updateChannel.Publish(msg);
                Monitor.PulseAll(_lock);
                return listening;
            }
        }

        public void Clear()
        {
            _list.Clear();
        }
    }
}