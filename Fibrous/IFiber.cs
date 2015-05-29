namespace Fibrous
{
    using System;

    /// <summary>
    /// Fibers are execution contexts that use Threads or ThreadPools for work handlers
    /// </summary>
    public interface IFiber : IExecutionContext, IScheduler, IDisposableRegistry
    {
        IFiber Start();
        void Stop();
    }

    public interface IExecutionContext
    {
        void Enqueue(Action action);
    }

    public interface IScheduler
    {
        IDisposable Schedule(Action action, TimeSpan dueTime);
        IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval);
    }

    public interface IDisposableRegistry : IDisposable
    {
        void Add(IDisposable toAdd);
        void Remove(IDisposable toRemove);
    }
}