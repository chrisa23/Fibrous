using System;

namespace Fibrous.Scheduling
{
    internal abstract class BatchSubscriberBase<T> : IDisposable
    {
        protected readonly object BatchLock = new object();
        protected readonly IFiber Fiber;
        protected readonly TimeSpan Interval;
        protected readonly IScheduler Scheduler;
        private readonly IDisposable _sub;

        protected BatchSubscriberBase(ISubscribePort<T> channel, IFiber fiber, IScheduler scheduler, TimeSpan interval)
        {
            Scheduler = scheduler;
            _sub = channel.Subscribe(fiber, OnMessage);
            Fiber = fiber;
            Interval = interval;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sub.Dispose();
        }

        #endregion

        protected abstract void OnMessage(T msg);
    }
}