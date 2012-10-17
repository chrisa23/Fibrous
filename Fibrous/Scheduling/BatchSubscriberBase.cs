namespace Fibrous.Scheduling
{
    using System;

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

        public void Dispose()
        {
            _sub.Dispose();
        }

        protected abstract void OnMessage(T msg);
    }
}