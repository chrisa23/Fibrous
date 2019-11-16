using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous
{
    internal sealed class AsyncTimerAction : IDisposable
    {
        private readonly Func<Task> _action;
        private readonly TimeSpan _interval;
        private bool _cancelled;
        private Timer _timer;

        public AsyncTimerAction(IAsyncFiber fiber, Func<Task> action, TimeSpan dueTime)
        {
            _action = action;
            _interval = TimeSpan.FromMilliseconds(-1);
            _timer = new Timer(x => ExecuteOnTimerThread(fiber), null, dueTime, _interval);
            fiber.Add(this);
        }

        public AsyncTimerAction(IAsyncFiber fiber, Func<Task> action, TimeSpan dueTime, TimeSpan interval)
        {
            _action = action;
            _interval = interval;
            _timer = new Timer(x => ExecuteOnTimerThread(fiber), null, dueTime, interval);
            fiber.Add(this);
        }

        public void Dispose()
        {
            _cancelled = true;
            DisposeTimer();
        }

        private void ExecuteOnTimerThread(IAsyncFiber fiber)
        {
            if (_interval.Ticks == TimeSpan.FromMilliseconds(-1).Ticks || _cancelled)
            {
                fiber.Remove(this);
                DisposeTimer();
            }

            if (!_cancelled)
                fiber.Enqueue(Execute);
        }

        private Task Execute()
        {
            if (_cancelled)
                return Task.CompletedTask;
            return _action();
        }

        private void DisposeTimer()
        {
            if (_timer == null) return;
            _timer.Dispose();
            _timer = null;
        }
    }
}