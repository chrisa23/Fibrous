using System;
using System.Threading.Tasks;
using Quartz;

namespace Fibrous;

internal class CronScheduler : IDisposable
{
    private readonly Func<Task> _action;
    private readonly CronExpression _cronExpression;
    private readonly object _gate = new();
    private readonly IScheduler _scheduler;
    private bool _running = true;
    private IDisposable _subscription;

    public CronScheduler(IScheduler scheduler, Func<Task> action, string cron)
    {
        _scheduler = scheduler;
        _action = async () =>
        {
            if (!IsRunning())
            {
                return;
            }

            await action();
            await ScheduleNextAsync();
        };

        _cronExpression = new CronExpression(cron);
        _ = ScheduleNextAsync();
    }

    public void Dispose()
    {
        IDisposable subscription;
        lock (_gate)
        {
            _running = false;
            subscription = _subscription;
            _subscription = null;
        }

        subscription?.Dispose();
    }

    private Task ScheduleNextAsync()
    {
        DateTimeOffset now = SystemTime.Now();
        DateTimeOffset? next = _cronExpression.GetNextValidTimeAfter(now);
        if (!next.HasValue)
        {
            return Task.CompletedTask;
        }

        TimeSpan dueTime = next.Value - now;
        if (dueTime < TimeSpan.Zero)
        {
            dueTime = TimeSpan.Zero;
        }

        IDisposable scheduled = _scheduler.Schedule(_action, dueTime);
        IDisposable previous;
        lock (_gate)
        {
            if (!_running)
            {
                scheduled.Dispose();
                return Task.CompletedTask;
            }

            previous = _subscription;
            _subscription = scheduled;
        }

        previous?.Dispose();
        return Task.CompletedTask;
    }

    private bool IsRunning()
    {
        lock (_gate)
        {
            return _running;
        }
    }
}
