using System;
using System.Threading;

namespace Fibrous;

internal sealed class TimerAction : IDisposable
{
    private readonly Action _action;
    private readonly TimeSpan _interval;
    private bool _cancelled;
    private Timer _timer;

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

    public void Dispose()
    {
        _cancelled = true;
        DisposeTimer();
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

    private void DisposeTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }
}
