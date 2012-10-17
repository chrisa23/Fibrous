namespace Fibrous.Scheduling
{
    using System;
    using System.Collections.Generic;

    internal sealed class KeyedBatchSubscriber<TKey, T> : BatchSubscriberBase<T>
    {
        private readonly Converter<T, TKey> _keyResolver;
        private readonly Action<IDictionary<TKey, T>> _target;
        private Dictionary<TKey, T> _pending;

        public KeyedBatchSubscriber(ISubscribePort<T> channel,
                                    IFiber fiber,
                                    IScheduler scheduler,
                                    TimeSpan interval,
                                    Converter<T, TKey> keyResolver,
                                    Action<IDictionary<TKey, T>> target)
            : base(channel, fiber, scheduler, interval)
        {
            _keyResolver = keyResolver;
            _target = target;
        }

        protected override void OnMessage(T msg)
        {
            lock (BatchLock)
            {
                TKey key = _keyResolver(msg);
                if (_pending == null)
                {
                    _pending = new Dictionary<TKey, T>();
                    Scheduler.Schedule(Fiber, Flush, Interval);
                }
                _pending[key] = msg;
            }
        }

        private void Flush()
        {
            IDictionary<TKey, T> toReturn = ClearPending();
            if (toReturn != null)
                _target(toReturn);
        }

        private IDictionary<TKey, T> ClearPending()
        {
            lock (BatchLock)
            {
                if (_pending == null || _pending.Count == 0)
                {
                    _pending = null;
                    return null;
                }
                IDictionary<TKey, T> toReturn = _pending;
                _pending = null;
                return toReturn;
            }
        }
    }
}