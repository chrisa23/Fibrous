namespace Fibrous
{
    using System;
    using Fibrous.Scheduling;

    /// <summary>
    ///   Enqueues pending actions for the context of execution (thread, pool of threads, message pump, etc.)
    /// </summary>
    public interface IFiber : IStartable, IExecutionContext, IDisposableRegistry
    {
    }

    public static class ExtensionsToIFiber
    {
        private static readonly TimerScheduler Scheduler = new TimerScheduler();

        /// <summary>   An IFiber extension method that schedules actions to occur at certain times. </summary>
        /// <param name="fiber">    The fiber to act on. </param>
        /// <param name="action">   The action. </param>
        /// <param name="dueTime">  Time of the due. </param>
        /// <returns> IDisposable for turning off the scheduling </returns>
        public static IDisposable Schedule(this IFiber fiber, Action action, TimeSpan dueTime)
        {
            return Scheduler.Schedule(fiber, action, dueTime);
        }

        /// <summary>   An IFiber extension method that schedules actions to occur on an interval. </summary>
        /// <param name="fiber">    The fiber to act on. </param>
        /// <param name="action">   The action. </param>
        /// <param name="dueTime">  Initial due time. </param>
        /// <param name="interval"> The interval. </param>
        /// <returns> IDisposable for turning off the scheduling </returns>
        public static IDisposable Schedule(this IFiber fiber, Action action, TimeSpan dueTime, TimeSpan interval)
        {
            return Scheduler.Schedule(fiber, action, dueTime, interval);
        }
    }
}