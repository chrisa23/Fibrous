using System;
using System.Threading;
using Fibrous.Internal;

namespace Fibrous.Scheduling
{
    public sealed class TimerScheduler : IScheduler
    {
        public IDisposable Schedule(IFiber fiber, Action action, long firstInMs)
        {
            if (firstInMs <= 0)
            {
                var pending = new PendingAction(action);
                fiber.Enqueue(pending.Execute);
                return pending;
            }
            return new TimerAction(fiber, action, firstInMs);
        }

        public IDisposable ScheduleOnInterval(IFiber fiber, Action action, long firstInMs, long regularInMs)
        {
            return new TimerAction(fiber, action, firstInMs, regularInMs);
        }

        public sealed class TimerAction : IDisposable
        {
            private readonly Action _action;
            private readonly long _intervalInMs;

            private Timer _timer;
            private bool _cancelled;

            public TimerAction(IFiber fiber, Action action, long firstIntervalInMs, long intervalInMs = Timeout.Infinite)
            {
                _action = action;
                _intervalInMs = intervalInMs;
                _timer = new Timer(x => ExecuteOnTimerThread(fiber), null, firstIntervalInMs, intervalInMs);
                fiber.Add(this);
            }

            private void ExecuteOnTimerThread(IFiber fiber)
            {
                if (_intervalInMs == Timeout.Infinite || _cancelled)
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
    }
}