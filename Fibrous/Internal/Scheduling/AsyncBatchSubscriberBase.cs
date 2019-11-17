using System;
using System.Threading.Tasks;

namespace Fibrous
{
    internal abstract class AsyncBatchSubscriberBase<T> : IDisposable
    {
        private readonly IDisposable _sub;
        protected readonly object BatchLock = new object();
        protected readonly IAsyncFiber Fiber;
        protected readonly TimeSpan Interval;

        protected AsyncBatchSubscriberBase(ISubscriberPort<T> channel, IAsyncFiber fiber, TimeSpan interval)
        {
            _sub = channel.Subscribe(fiber, OnMessage);
            Fiber = fiber;
            Interval = interval;
        }

        public void Dispose()
        {
            _sub.Dispose();
        }

        protected abstract Task OnMessage(T msg);
    }
}