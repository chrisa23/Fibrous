using System;
using Quartz;

namespace Fibrous;

internal class CronScheduler : IDisposable
{
    private readonly Action _action;
    private readonly CronExpression _cronExpression;
    private readonly IScheduler _scheduler;
    private bool _running = true;
    private IDisposable _sub;

    public CronScheduler(IScheduler scheduler, Action action, string cron)
    {
        _cronExpression = new CronExpression(cron);

        _scheduler = scheduler;
        _action = () =>
        {
            action();
            ScheduleNext();
        };

        ScheduleNext();
    }

    public void Dispose()
    {
#if DEBUG
        Console.WriteLine("Dispose");
#endif
        _sub?.Dispose();
        _running = false;
    }

    private void ScheduleNext()
    {
        if (!_running)
        {
            return;
        }

        DateTimeOffset? next = _cronExpression.GetNextValidTimeAfter(DateTimeOffset.Now);
        if (next.HasValue)
        {
            DateTime utc = next.Value.UtcDateTime;
            DateTime now = DateTime.UtcNow;
            TimeSpan span = utc - now;

            if (!_running)
            {
                return;
            }
#if DEBUG
            Console.WriteLine(span);
#endif
            _sub = _scheduler.Schedule(_action, span);
        }
    }
}
