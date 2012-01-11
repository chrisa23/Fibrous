using System;
using System.Collections.Generic;

namespace Fibrous.Scheduling
{
    public sealed class BatchSubscriber<T> : BatchSubscriberBase<T>
    {
        private readonly Action<IList<T>> _receive;
        private List<T> _pending;

        public BatchSubscriber(ISubscriberPort<T> channel, IFiber fiber, IScheduler scheduler, TimeSpan interval,
                               Action<IList<T>> receive)
            : base(channel, fiber, scheduler, interval)
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
                    Scheduler.Schedule(Fiber, Flush, (long) Interval.TotalMilliseconds);
                }
                _pending.Add(msg);
            }
        }

        private void Flush()
        {
            IList<T> toFlush = null;
            lock (BatchLock)
            {
                if (_pending != null)
                {
                    toFlush = _pending;
                    _pending = null;
                }
            }
            if (toFlush != null)
            {
                _receive(toFlush);
            }
        }
    }
}