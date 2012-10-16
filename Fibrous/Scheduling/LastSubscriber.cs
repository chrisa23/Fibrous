using System;

namespace Fibrous.Scheduling
{
    internal sealed class LastSubscriber<T> : BatchSubscriberBase<T>
    {
        private readonly Action<T> _target;
        private bool _flushPending;
        private T _pending;

        public LastSubscriber(ISubscribePort<T> channel,
                              IFiber fiber,
                              IScheduler scheduler,
                              TimeSpan interval,
                              Action<T> target)
            : base(channel, fiber, scheduler, interval)
        {
            _target = target;
        }

        protected override void OnMessage(T msg)
        {
            lock (BatchLock)
            {
                if (!_flushPending)
                {
                    Scheduler.Schedule(Fiber, Flush, Interval);
                    _flushPending = true;
                }
                _pending = msg;
            }
        }

        private void Flush()
        {
            T toReturn = ClearPending();
            _target(toReturn);
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