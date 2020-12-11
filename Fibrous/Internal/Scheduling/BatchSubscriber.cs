using System;
using System.Collections.Generic;

namespace Fibrous
{
    internal sealed class BatchSubscriber<T> : BatchSubscriberBase<T>
    {
        private readonly List<T> _batch = new List<T>();
        private readonly Action<T[]> _receive;
        private bool _pending;

        public BatchSubscriber(ISubscriberPort<T> channel,
            IFiber fiber,
            TimeSpan interval,
            Action<T[]> receive)
            : base(channel, fiber, interval) =>
            _receive = receive;

        //This type of batching basically puts a lag on receiving
        //and doesn't maintain a steady batch rate...
        //
        protected override void OnMessage(T msg)
        {
            lock (BatchLock)
            {
                if (!_pending)
                {
                    _pending = true;
                    Fiber.Schedule(Flush, Interval);
                }

                _batch.Add(msg);
            }
        }

        private void Flush()
        {
            T[] toFlush = null;
            lock (BatchLock)
            {
                if (_pending)
                {
                    toFlush = _batch.ToArray();
                    _batch.Clear();
                    _pending = false;
                }
            }

            if (toFlush != null)
            {
                Fiber.Enqueue(() => _receive(toFlush));
            }
        }
    }
}
