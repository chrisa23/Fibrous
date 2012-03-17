namespace Fibrous.Scheduling
{
    using System;
    using System.Threading;

    public sealed class TimerScheduler : IScheduler
    {
        public IDisposable Schedule(IFiber fiber, Action action, TimeSpan dueTime)
        {
            if (dueTime.TotalMilliseconds <= 0)
            {
                var pending = new PendingAction(action);
                fiber.Enqueue(pending.Execute);
                return pending;
            }
            return new TimerAction(fiber, action, dueTime);
        }

        public IDisposable Schedule(IFiber fiber, Action action, TimeSpan dueTime, TimeSpan interval)
        {
            return new TimerAction(fiber, action, dueTime, interval);
        }

        internal sealed class TimerAction : IDisposable
        {
            private readonly Action _action;
            private readonly TimeSpan _interval;
            private Timer _timer;
            private bool _cancelled;

            public TimerAction(IFiber fiber, Action action, TimeSpan dueTime)
            {
                _action = action;
                _interval = TimeSpan.FromMilliseconds(-1);
                _timer = new Timer(x => ExecuteOnTimerThread(fiber), null, dueTime, _interval);
                fiber.Add(this);
            }

            public TimerAction(IFiber fiber, Action action, TimeSpan dueTime, TimeSpan interval)
            {
                _action = action;
                _interval = interval;
                _timer = new Timer(x => ExecuteOnTimerThread(fiber), null, dueTime, interval);
                fiber.Add(this);
            }

            private void ExecuteOnTimerThread(IFiber fiber)
            {
                if (_interval.Ticks == TimeSpan.FromMilliseconds(-1).Ticks || _cancelled)
                {
                    fiber.Remove(this);
                    DisposeTimer();
                }
                if (!_cancelled)
                {
                    fiber.Enqueue(Execute);
                }
            }

            private void Execute()
            {
                if (!_cancelled)
                {
                    _action();
                }
            }

            public void Dispose()
            {
                _cancelled = true;
                DisposeTimer();
            }

            private void DisposeTimer()
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        internal sealed class PendingAction : IDisposable
        {
            private readonly Action _action;
            private bool _cancelled;

            public PendingAction(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _cancelled = true;
            }

            public void Execute()
            {
                if (!_cancelled)
                {
                    _action();
                }
            }
        }
    }
}