namespace Fibrous.Experimental
{
    using System;
    using System.Threading;
    using Fibrous.Channels;

    public sealed class StateChannel<T> : IChannel<T>
    {
        private T _last;
        private bool _hasValue;
        private readonly object _lock = new object();
        private readonly IChannel<T> _updateChannel = new Channel<T>();

        public IDisposable Subscribe(IFiber fiber, Action<T> handler)
        {
            lock (_lock)
            {
                IDisposable disposable = _updateChannel.Subscribe(fiber, handler);
                if (_hasValue)
                {
                    T item = _last;
                    fiber.Enqueue(() => handler(item));
                }
                return disposable;
            }
        }

        public bool Publish(T msg)
        {
            lock (_lock)
            {
                _last = msg;
                _hasValue = true;
                bool publish = _updateChannel.Publish(msg);
                Monitor.PulseAll(_lock);
                return publish;
            }
        }
    }
}