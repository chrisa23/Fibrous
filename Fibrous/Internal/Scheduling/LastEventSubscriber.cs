using System;
using System.Threading;

namespace Fibrous
{
    internal class LastEventSubscriber : IDisposable
    {
        private readonly Action _callback;
        private readonly IFiber _fiber;
        private readonly TimeSpan _interval;
        private readonly IDisposable _sub;
        private int _flushPending;

        public LastEventSubscriber(IEventPort channel, IFiber fiber, TimeSpan interval, Action callback)
        {
            _sub = channel.Subscribe(fiber, OnEvent);
            _fiber = fiber;
            _interval = interval;
            _callback = callback;
        }

        public void Dispose() => _sub.Dispose();

        private void OnEvent()
        {
            if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1)
            {
                return;
            }

            _fiber.Schedule(Flush, _interval);
        }

        private void Flush()
        {
            Interlocked.Exchange(ref _flushPending, 0);
            _fiber.Enqueue(_callback);
        }
    }
}
