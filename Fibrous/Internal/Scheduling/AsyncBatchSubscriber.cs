using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous
{
    internal sealed class AsyncBatchSubscriber<T> : AsyncBatchSubscriberBase<T>
    {
        private readonly Func<T[], Task> _receive;
        private List<T> _pending;

        public AsyncBatchSubscriber(ISubscriberPort<T> channel,
            IAsyncFiber fiber,
            TimeSpan interval,
            Func<T[], Task> receive)
            : base(channel, fiber, interval) =>
            _receive = receive;

        protected override Task OnMessageAsync(T msg)
        {
            lock (BatchLock)
            {
                if (_pending == null)
                {
                    _pending = new List<T>();
                    Fiber.Schedule(FlushAsync, Interval);
                }

                _pending.Add(msg);
            }

            return Task.CompletedTask;
        }

        private Task FlushAsync()
        {
            T[] toFlush = null;
            lock (BatchLock)
            {
                if (_pending != null)
                {
                    toFlush = _pending.ToArray();
                    _pending = null;
                }
            }

            if (toFlush != null)
            {
                Fiber.Enqueue(() => _receive(toFlush));
            }

            return Task.CompletedTask;
        }
    }
}
