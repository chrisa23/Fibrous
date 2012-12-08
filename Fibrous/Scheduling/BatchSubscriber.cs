namespace Fibrous.Scheduling
{
    using System;
    using System.Collections.Generic;

    internal sealed class BatchSubscriber<T> : BatchSubscriberBase<T>
    {
        private readonly Action<T[]> _receive;
        private List<T> _pending;

        public BatchSubscriber(ISubscriberPort<T> channel,
                               Fiber fiber,
                               TimeSpan interval,
                               Action<T[]> receive)
            : base(channel, fiber, interval)
        {
            _receive = receive;
        }

        protected override void OnMessage(T msg)
        {
            lock (BatchLock)
            {
                if (_pending == null)
                {
                    _pending = new List<T>();
                    Fiber.Schedule(Flush, Interval);
                }
                _pending.Add(msg);
            }
        }

        private void Flush()
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
                _receive(toFlush);
        }
    }
}