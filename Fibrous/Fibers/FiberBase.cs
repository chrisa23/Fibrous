namespace Fibrous.Fibers
{
    using System;
    using Fibrous.Scheduling;
    using Fibrous.Utility;

    public abstract class FiberBase : Disposables, IFiber
    {
        protected readonly IExecutor Executor;
        private readonly IFiberScheduler _fiberScheduler;

        protected FiberBase(IExecutor executor, IFiberScheduler scheduler)
        {
            _fiberScheduler = scheduler;
            Executor = executor;
        }

        protected FiberBase(IExecutor executor)
        {
            _fiberScheduler = new TimerScheduler();
            Executor = executor;
        }

        protected FiberBase()
            : this(new DefaultExecutor(), new TimerScheduler())
        {
        }

        public abstract IFiber Start();
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