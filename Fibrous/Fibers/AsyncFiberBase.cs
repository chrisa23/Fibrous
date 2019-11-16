using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous
{
    public abstract class AsyncFiberBase : Disposables, IAsyncFiber
    {
        private readonly IAsyncFiberScheduler _fiberScheduler;
        private readonly List<Func<Task>> _preQueue = new List<Func<Task>>();

        protected readonly IAsyncExecutor Executor;
        private ExecutionState _started = ExecutionState.Created;

        protected AsyncFiberBase(IAsyncExecutor executor, IAsyncFiberScheduler scheduler)
        {
            _fiberScheduler = scheduler;
            Executor = executor;
        }

        protected AsyncFiberBase(IAsyncExecutor executor)
        {
            _fiberScheduler = new AsyncTimerScheduler();
            Executor = executor;
        }

        protected AsyncFiberBase()
            : this(new AsyncExecutor(), new AsyncTimerScheduler())
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
                    for (var i = 0; i < _preQueue.Count; i++)
                        InternalEnqueue(_preQueue[i]);
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

        public void Enqueue(Func<Task> action)
        {
            if (_started == ExecutionState.Stopped)
                return;
            if (_started == ExecutionState.Created)
                lock (_preQueue)
                {
                    if (_started == ExecutionState.Created)
                    {
                        _preQueue.Add(action);
                        return;
                    }
                }

            InternalEnqueue(action);
        }

        public IDisposable Schedule(Func<Task> action, TimeSpan dueTime)
        {
            return _fiberScheduler.Schedule(this, action, dueTime);
        }

        public IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval)
        {
            return _fiberScheduler.Schedule(this, action, startTime, interval);
        }

        protected virtual void InternalStart()
        {
        }

        protected abstract void InternalEnqueue(Func<Task> action);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _started = ExecutionState.Stopped;
            base.Dispose(disposing);
        }

        private enum ExecutionState
        {
            Created,
            Running,
            Stopped
        }
    }
}