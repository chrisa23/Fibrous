using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IAsyncFiber : IAsyncScheduler, IDisposableRegistry, IAsyncExecutionContext
    {
        /// <summary>
        ///     Start the fiber's queue
        /// </summary>
        /// <returns></returns>
        void Start();

        /// <summary>
        ///     Stop the fiber
        /// </summary>
        void Stop();
    }

    public interface IAsyncScheduler
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
    }


    public interface IAsyncExecutionContext
    {
        void Enqueue(Func<Task> action);
    }


    public interface IAsyncExecutor
    {
        Task Execute(Func<Task> toExecute);
        Task Execute(int count, Func<Task>[] actions);
    }
}