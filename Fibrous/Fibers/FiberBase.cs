namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    public abstract class FiberBase : Disposables, IFiber
    {
        private enum ExecutionState
        {
            Created,
            Running,
            Stopped
        }

        protected readonly Executor Executor;
        private readonly IFiberScheduler _fiberScheduler;
        private ExecutionState _started = ExecutionState.Created;
        private readonly List<Action> _preQueue = new List<Action>();

        protected FiberBase(Executor executor, IFiberScheduler scheduler)
        {
            _fiberScheduler = scheduler;
            Executor = executor;
        }

        protected FiberBase(Executor executor)
        {
            _fiberScheduler = new TimerScheduler();
            Executor = executor;
        }

        protected FiberBase()
            : this(new Executor(), new TimerScheduler())
        {
        }

        protected virtual void InternalStart()
        {
        }

        public void Start()
        {
            if (_started == ExecutionState.Running) return; //??just ignore.  why explode?
            InternalStart();
            lock (_preQueue)
            {
                if (_preQueue.Count > 0)
                    InternalEnqueue(() => Executor.Execute(_preQueue));
                _started = ExecutionState.Running;
            }
        }

        public void Stop()
        {
            if (_started != ExecutionState.Running) return; //??just ignore.  why explode?
            lock (_preQueue)
            {
                _started = ExecutionState.Created;
            }
        }

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