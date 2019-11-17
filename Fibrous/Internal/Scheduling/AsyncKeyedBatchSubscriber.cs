using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous
{
    internal sealed class AsyncKeyedBatchSubscriber<TKey, T> : AsyncBatchSubscriberBase<T>
    {
        private readonly Converter<T, TKey> _keyResolver;
        private readonly Func<IDictionary<TKey, T>, Task> _target;
        private Dictionary<TKey, T> _pending;

        public AsyncKeyedBatchSubscriber(ISubscriberPort<T> channel,
            IAsyncFiber fiber,
            TimeSpan interval,
            Converter<T, TKey> keyResolver,
            Func<IDictionary<TKey, T>, Task> target)
            : base(channel, fiber, interval)
        {
            _keyResolver = keyResolver;
            _target = target;
        }

        protected override Task OnMessage(T msg)
        {
            lock (BatchLock)
            {
                var key = _keyResolver(msg);
                if (_pending == null)
                {
                    _pending = new Dictionary<TKey, T>();
                    Fiber.Schedule(Flush, Interval);
                }

                _pending[key] = msg;
            }

            return Task.CompletedTask;
        }

        private Task Flush()
        {
            var toReturn = ClearPending();
            if (toReturn != null)
                Fiber.Enqueue(() => _target(toReturn));
            return Task.CompletedTask;
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