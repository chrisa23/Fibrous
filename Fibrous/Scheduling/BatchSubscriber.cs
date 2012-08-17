namespace Fibrous.Scheduling
{
    using System;
    using System.Collections.Generic;

    internal sealed class BatchSubscriber<T> : BatchSubscriberBase<T>
    {
        private readonly Action<IList<T>> _receive;
        private List<T> _pending;

        public BatchSubscriber(ISubscribePort<T> channel,
                               IFiber fiber,
                               IScheduler scheduler,
                               TimeSpan interval,
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
                    Scheduler.Schedule(Fiber, Flush, Interval);
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