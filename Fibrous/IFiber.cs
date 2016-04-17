namespace Fibrous
{
    using System;

    /// <summary>
    /// Fibers are execution contexts that use Threads or ThreadPools for work handlers
    /// </summary>
    public interface IFiber : IExecutionContext, IScheduler, IDisposableRegistry
    {
        /// <summary>
        /// Start the fiber's queue
        /// </summary>
        /// <returns></returns>
        void Start();

        /// <summary>
        /// Stop the fiber
        /// </summary>
        void Stop();
    }
    
    public interface IExecutionContext
    {
        /// <summary>
        /// Enqueue an Action to be executed
        /// </summary>
        /// <param name="action"></param>
        void Enqueue(Action action);
    }

    public interface IScheduler
    {
        /// <summary>
        /// Schedule an action to be executed once
        /// </summary>
        /// <param name="action"></param>
        /// <param name="dueTime"></param>
        /// <returns></returns>
        IDisposable Schedule(Action action, TimeSpan dueTime);

        /// <summary>
        /// Schedule an action to be taken repeatedly
        /// </summary>
        /// <param name="action"></param>
        /// <param name="startTime"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval);
    }

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

    /// <summary>
    /// Collection of disposables, where they can be removed or Disposed together.  
    /// Mostly for internal use, but very convenient for grouping and handling disposables
    /// </summary>
    public interface IDisposableRegistry : IDisposable
    {
        /// <summary>
        /// Add an IDisposable to the registry.  It will be disposed when the registry is disposed.
        /// </summary>
        /// <param name="toAdd"></param>
        void Add(IDisposable toAdd);

        /// <summary>
        /// Remove a disposable from the registry.  It will not be disposed when the registry is disposed.
        /// </summary>
        /// <param name="toRemove"></param>
        void Remove(IDisposable toRemove);
    }
}