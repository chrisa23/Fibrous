using System;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Channel that maintains its last value which is passed to new subscribers.  Useful with Enums or values representing
    ///     latest status.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class StateChannel<T> : IChannel<T>
    {
        private readonly object _lock = new object();
        private readonly IChannel<T> _updateChannel = new Channel<T>();
        private bool _hasValue;
        private T _last;

        public StateChannel(T initial)
        {
            _last = initial;
            _hasValue = true;
        }

        public StateChannel()
        {
        }

        public bool HasValue
        {
            get
            {
                lock (_lock)
                {
                    return _hasValue;
                }
            }
        }

        public T Current
        {
            get
            {
                lock (_lock)
                {
                    return _last;
                }
            }
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> handler)
        {
            lock (_lock)
            {
                var disposable = _updateChannel.Subscribe(fiber, handler);
                if (_hasValue)
                {
                    var item = _last;
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
                if (_hasValue)
                {
                    var item = _last;
                    fiber.Enqueue(() => receive(item));
                }

                return disposable;
            }
        }

        public void Publish(T msg)
        {
            lock (_lock)
            {
                _last = msg;
                _hasValue = true;
                _updateChannel.Publish(msg);
            }
        }
    }
}