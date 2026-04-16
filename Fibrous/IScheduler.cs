using System;
using System.Threading.Tasks;

namespace Fibrous;

public interface IScheduler
{
    /// <summary>
    ///     Schedules an action to execute once.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="dueTime">Delay before execution.</param>
    IDisposable Schedule(Func<Task> action, TimeSpan dueTime);

    /// <summary>
    ///     Schedules an action to execute repeatedly.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="startTime">Initial delay before the first execution.</param>
    /// <param name="interval">Interval between executions.</param>
    IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval);

    /// <summary>
    ///     Schedules an action to execute once.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="dueTime">Delay before execution.</param>
    IDisposable Schedule(Action action, TimeSpan dueTime);

    /// <summary>
    ///     Schedules an action to execute repeatedly.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="startTime">Initial delay before the first execution.</param>
    /// <param name="interval">Interval between executions.</param>
    IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval);
}
