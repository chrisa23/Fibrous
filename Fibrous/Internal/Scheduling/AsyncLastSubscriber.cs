using System;
using System.Threading.Tasks;

namespace Fibrous
{
    internal sealed class AsyncLastSubscriber<T> : AsyncBatchSubscriberBase<T>
    {
        private readonly Func<T, Task> _target;
        private bool _flushPending;
        private T _pending;

        public AsyncLastSubscriber(ISubscriberPort<T> channel,
            IAsyncFiber fiber,
            TimeSpan interval,
            Func<T, Task> target)
            : base(channel, fiber, interval) =>
            _target = target;

        protected override Task OnMessageAsync(T msg)
        {
            lock (BatchLock)
            {
                if (!_flushPending)
                {
                    Fiber.Schedule(FlushAsync, Interval);
                    _flushPending = true;
                }

                _pending = msg;
            }

            return Task.CompletedTask;
        }

        private Task FlushAsync()
        {
            T toReturn = ClearPending();
            Fiber.Enqueue(() => _target(toReturn));
            return Task.CompletedTask;
        }

        private T ClearPending()
        {
            lock (BatchLock)
            {
                _flushPending = false;
                return _pending;
            }
        }
    }
}
