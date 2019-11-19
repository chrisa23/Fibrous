using System;
using System.Threading.Tasks;
using Quartz;

namespace Fibrous
{
    internal class AsyncCronScheduler : IDisposable
    {
        private readonly Func<Task> _action;
        private readonly CronExpression _cronExpression;
        private readonly IAsyncScheduler _scheduler;
        private bool _running = true;
        private IDisposable _sub;

        //make use of current timespan scheduling 
        //but with UTC and add hour on ambiguous when time < now
        public AsyncCronScheduler(IAsyncScheduler scheduler, Func<Task> action, string cron)
        {
            _scheduler = scheduler;
            _action = async () =>
            {
                await action();
                await ScheduleNext();
            };
            //parse cron
            //find next and schedule
            //on next, repeat
            //TODO:  try parse without and then with seconds
            _cronExpression = new CronExpression(cron);
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

        private Task ScheduleNext()
        {
            if (!_running) return Task.CompletedTask;
            var next = _cronExpression.GetNextValidTimeAfter(DateTimeOffset.Now);
            if (next.HasValue)
            {
                var utc = next.Value.UtcDateTime;
                //                var isAmbiguous = TimeZoneInfo.Local.IsAmbiguousTime(next);
                var now = DateTime.UtcNow;
                //              if (isAmbiguous && utc < now)
                //utc = utc.AddHours(1);
                var span = utc - now;

                if (!_running) return Task.CompletedTask;
#if DEBUG
                Console.WriteLine(span);
#endif
                _sub = _scheduler.Schedule(_action, span);
            }

            return Task.CompletedTask;
        }
    }
}