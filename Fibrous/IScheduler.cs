using System;
using System.Threading.Tasks;

namespace Fibrous;

public interface IScheduler
{
    /// <summary>
    ///     Schedule an action to be executed once
    /// </summary>
    /// <param name="action"></param>
    /// <param name="dueTime"></param>
    /// <returns></returns>
    IDisposable Schedule(Func<Task> action, TimeSpan dueTime);

    /// <summary>
    ///     Schedule an action to be taken repeatedly
    /// </summary>
    /// <param name="action"></param>
    /// <param name="startTime"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval);

    /// <summary>
    ///     Schedule an action to be executed once
    /// </summary>
    /// <param name="action"></param>
    /// <param name="dueTime"></param>
    /// <returns></returns>
    IDisposable Schedule(Action action, TimeSpan dueTime);

    /// <summary>
    ///     Schedule an action to be taken repeatedly
    /// </summary>
    /// <param name="action"></param>
    /// <param name="startTime"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval);
}
