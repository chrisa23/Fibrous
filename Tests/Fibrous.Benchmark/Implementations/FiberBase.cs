using System;
using System.Collections.Generic;

namespace Fibrous
{
    public abstract class FiberBase_old : Disposables, IFiber
    {
        private readonly IFiberScheduler _fiberScheduler;
        private readonly List<Action> _preQueue = new();

        protected readonly IExecutor Executor;
        private ExecutionState _started = ExecutionState.Created;

        protected FiberBase_old(IExecutor executor, IFiberScheduler scheduler)
        {
            _fiberScheduler = scheduler;
            Executor = executor;
        }

        protected FiberBase_old(IExecutor executor)
        {
            _fiberScheduler = new TimerScheduler();
            Executor = executor;
        }

        protected FiberBase_old()
            : this(new Executor(), new TimerScheduler())
        {
        }

        public void Enqueue(Action action)
        {
            if (_started == ExecutionState.Stopped)
            {
                return;
            }

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

        public IDisposable Schedule(Action action, TimeSpan dueTime) => _fiberScheduler.Schedule(this, action, dueTime);

        public IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval) =>
            _fiberScheduler.Schedule(this, action, startTime, interval);

        public override void Dispose()
        {
            _started = ExecutionState.Stopped;
            base.Dispose();
        }

        public IFiber Start()
        {
            if (_started == ExecutionState.Running)
            {
                return this;
            }

            InternalStart();
            lock (_preQueue)
            {
                _started = ExecutionState.Running;
                if (_preQueue.Count > 0)
                {
                    for (int i = 0; i < _preQueue.Count; i++)
                    {
                        InternalEnqueue(_preQueue[i]);
                    }
                }
            }

            return this;
        }

        public void Stop()
        {
            if (_started != ExecutionState.Running)
            {
                return;
            }

            lock (_preQueue)
            {
                _started = ExecutionState.Created;
            }
        }

        protected virtual void InternalStart()
        {
        }

        protected abstract void InternalEnqueue(Action action);
    }

    internal enum ExecutionState
    {
        Created,
        Running,
        Stopped
    }
}
