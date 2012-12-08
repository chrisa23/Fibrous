namespace Fibrous
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Scheduling;

    public abstract class Fiber : Disposables, IExecutionContext, IScheduler
    {
        //include this in Thread fiber...
        internal enum ExecutionState
        {
            Created,
            Running,
            Stopped
        }

        protected readonly Executor Executor;
        private readonly IFiberScheduler _fiberScheduler;
        private ExecutionState _started = ExecutionState.Created;
        private readonly List<Action> _preQueue = new List<Action>();

        protected Fiber(Executor executor, IFiberScheduler scheduler)
        {
            _fiberScheduler = scheduler;
            Executor = executor;
        }

        protected Fiber(Executor executor)
        {
            _fiberScheduler = new TimerScheduler();
            Executor = executor;
        }

        protected Fiber()
            : this(new Executor(), new TimerScheduler())
        {
        }

        protected virtual void InternalStart()
        {
        }

        public Fiber Start()
        {
            if (_started == ExecutionState.Running)
                throw new ThreadStateException("Already Started");
            _started = ExecutionState.Running;
            InternalStart();
            lock (_preQueue)
            {
                if (_preQueue.Count > 0)
                {
                    Action[] actions = _preQueue.ToArray();
                    InternalEnqueue(() => Executor.Execute(actions));
                }
            }
            return this;
        }

        //how do I get the pre-execution queuing here...?
        public void Enqueue(Action action)
        {
            if (_started == ExecutionState.Stopped)
                return;
            if (_started == ExecutionState.Created)
            {
                lock (_preQueue)
                {
                    if (_started == ExecutionState.Created)
                    {
                        _preQueue.Add(action);
                        return;
                    }
                }
            }
            InternalEnqueue(action);
        }

        protected abstract void InternalEnqueue(Action action);

        public IDisposable Schedule(Action action, TimeSpan dueTime)
        {
            return _fiberScheduler.Schedule(this, action, dueTime);
        }

        public IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval)
        {
            return _fiberScheduler.Schedule(this, action, startTime, interval);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _started = ExecutionState.Stopped;
            base.Dispose(disposing);
        }
    }
}