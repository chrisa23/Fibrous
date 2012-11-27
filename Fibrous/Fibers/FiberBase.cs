namespace Fibrous.Fibers
{
    using System;
    using Fibrous.Scheduling;
    using Fibrous.Utility;

    public abstract class FiberBase : Disposables, IFiber
    {
        private readonly IFiberScheduler _fiberScheduler;
        protected readonly IExecutor Executor;

        protected FiberBase(FiberConfig config)
        {
            _fiberScheduler = config.FiberScheduler;
            Executor = config.Executor;
        }

        protected FiberBase()
            : this(FiberConfig.Default)
        {
        }

        public abstract void Start();
        public abstract void Enqueue(Action action);

        public IDisposable Schedule(Action action, TimeSpan dueTime)
        {
            return _fiberScheduler.Schedule(this, action, dueTime);
        }

        public IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval)
        {
            return _fiberScheduler.Schedule(this, action, startTime, interval);
        }
    }
}