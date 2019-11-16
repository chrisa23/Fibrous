using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Simple snapshot mechanism.  Replay all old messages on subscribe.
    ///     Can cause memory leaks if not handled with care
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
    public sealed class ReplayChannel<T> : IChannel<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _lock = new object();
        private readonly IChannel<T> _updateChannel = new Channel<T>();

        public IDisposable Subscribe(IFiber fiber, Action<T> handler)
        {
            lock (_lock)
            {
                var disposable = _updateChannel.Subscribe(fiber, handler);
                var length = _list.Count;
                for (var index = 0; index < length; index++)
                {
                    var item = _list[index];
                    fiber.Enqueue(() => handler(item));
                }

                return disposable;
            }
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<T, Task> receive)
        {
            lock (_lock)
            {
                var disposable = _updateChannel.Subscribe(fiber, receive);
                var length = _list.Count;
                for (var index = 0; index < length; index++)
                {
                    var item = _list[index];
                    fiber.Enqueue(() => receive(item));
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