using System;
using System.Collections.Generic;

namespace Fibrous
{
    public abstract class FiberBase : Disposables, IFiber
    {
        private readonly IFiberScheduler _fiberScheduler;
        private readonly List<Action> _preQueue = new List<Action>();

        protected readonly IExecutor Executor;
        private ExecutionState _started = ExecutionState.Created;

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
            : this(new Executor(), new TimerScheduler())
        {
        }

        public void Start()
        {
            if (_started == ExecutionState.Running) return; //??just ignore.  why explode?
            InternalStart();
            lock (_preQueue)
            {
                _started = ExecutionState.Running;
                if (_preQueue.Count > 0)
                {
                    for (var i = 0; i < _preQueue.Count; i++)
                    {
                        InternalEnqueue(_preQueue[i]);
                    }
                }
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

        public IDisposable Schedule(Action action, TimeSpan dueTime)
        {
            return _fiberScheduler.Schedule(this, action, dueTime);
        }

        public IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval)
        {
            return _fiberScheduler.Schedule(this, action, startTime, interval);
        }

        protected virtual void InternalStart()
        {
        }

        protected abstract void InternalEnqueue(Action action);

        public override void Dispose()
        {
            _started = ExecutionState.Stopped;
            base.Dispose();
        }

        private enum ExecutionState
        {
            Created,
            Running,
            Stopped
        }
    }
}