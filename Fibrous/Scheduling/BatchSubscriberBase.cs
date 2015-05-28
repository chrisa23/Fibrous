namespace Fibrous
{
    using System;

    internal abstract class BatchSubscriberBase<T> : IDisposable
    {
        protected readonly object BatchLock = new object();
        protected readonly IFiber Fiber;
        protected readonly TimeSpan Interval;
        private readonly IDisposable _sub;

        protected BatchSubscriberBase(ISubscriberPort<T> channel, IFiber fiber, TimeSpan interval)
        {
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