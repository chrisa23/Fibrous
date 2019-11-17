using System;
using System.Collections.Generic;
using System.Text;
using Quartz;

namespace Fibrous
{
    
    internal class CronScheduler :IDisposable
    {
        private readonly IScheduler _scheduler;
        private readonly Action _action;
        private IDisposable _sub;
        private bool _running = true;
        private readonly CronExpression _cronExpression;

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

        private void ScheduleNext()
        {
            if (!_running) return;

            var next = _cronExpression.GetNextValidTimeAfter(DateTimeOffset.Now);
            if (next.HasValue)
            {
                var utc = next.Value.UtcDateTime;
                //                var isAmbiguous = TimeZoneInfo.Local.IsAmbiguousTime(next);
                var now = DateTime.UtcNow;
                //              if (isAmbiguous && utc < now)
                //utc = utc.AddHours(1);
                var span = utc - now;

                if (!_running) return;
#if DEBUG
                Console.WriteLine(span);
#endif
                _sub = _scheduler.Schedule(_action, span);
            }

        }

        public void Dispose()
        {
#if DEBUG
            Console.WriteLine("Dispose");
#endif
            _sub?.Dispose();
            _running = false;
        }
    }
}
