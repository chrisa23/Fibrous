namespace Fibrous
{
    using System;

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
}