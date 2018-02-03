namespace Fibrous
{
    using System;

    public static class SchedulerExtensions
    {
        /// <summary>
        /// Schedule an action at a DateTime
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="action"></param>
        /// <param name="when"></param>
        /// <returns></returns>
        public static IDisposable Schedule(this IScheduler scheduler, Action action, DateTime when)
        {
            return scheduler.Schedule(action, when - DateTime.Now);
        }

        /// <summary>
        /// Schedule an action at a DateTime with an interval
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="action"></param>
        /// <param name="when"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IDisposable Schedule(this IScheduler scheduler, Action action, DateTime when, TimeSpan interval)
        {
            return scheduler.Schedule(action, when - DateTime.Now, interval);
        }
    }
}