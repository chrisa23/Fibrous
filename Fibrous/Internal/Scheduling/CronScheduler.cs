using System;
using System.Threading.Tasks;
using Quartz;

namespace Fibrous;

internal class CronScheduler : IDisposable
{
    private readonly Func<Task> _action;
    private readonly CronExpression _cronExpression;
    private readonly IScheduler _scheduler;
    private bool _running = true;
    private IDisposable _sub;

    //make use of current timespan scheduling
    //but with UTC and add hour on ambiguous when time < now
    public CronScheduler(IScheduler scheduler, Func<Task> action, string cron)
    {
        _scheduler = scheduler;
        _action = async () =>
        {
            await action();
            await ScheduleNextAsync();
        };
        //parse cron
        //find next and schedule
        //on next, repeat
        //TODO:  try parse without and then with seconds
        _cronExpression = new CronExpression(cron);
        _ = ScheduleNextAsync();
    }

    public void Dispose()
    {
        _running = false;
        _sub?.Dispose();
#if DEBUG
        Console.WriteLine("Dispose");
#endif
    }

    private Task ScheduleNextAsync()
    {
        if (!_running)
        {
            return Task.CompletedTask;
        }

        DateTimeOffset? next = _cronExpression.GetNextValidTimeAfter(DateTimeOffset.Now);
        if (next.HasValue)
        {
            DateTime utc = next.Value.UtcDateTime;
            DateTime now = DateTime.UtcNow;
            TimeSpan span = utc - now;

            if (!_running)
            {
                return Task.CompletedTask;
            }

            _sub = _scheduler.Schedule(_action, span);

#if DEBUG
            Console.WriteLine(span);
#endif
        }

        return Task.CompletedTask;
    }
}
