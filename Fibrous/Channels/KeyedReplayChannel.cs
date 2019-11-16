using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Replay channel that replays the last items stored by key
    ///     Can cause memory leaks if not handled with care
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="T"></typeparam>
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
                var disposable = _updateChannel.Subscribe(fiber, handler);
                foreach (var item in _list.Values)
                    handler(item);
                return disposable;
            }
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<T, Task> receive)
        {
            lock (_lock)
            {
                var disposable = _updateChannel.Subscribe(fiber, receive);

                foreach (var item in _list.Values)
                    fiber.Enqueue(() => receive(item));
                return disposable;
            }
        }

        public void Publish(T msg)
        {
            lock (_lock)
            {
                _list[_keyMaker(msg)] = msg;
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