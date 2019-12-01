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
        private ExecutionState _state = ExecutionState.Created;

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

        public IAsyncFiber Start()
        {
            if (_state == ExecutionState.Running) return this;
            InternalStart();
            lock (_preQueue)
            {
                _state = ExecutionState.Running;
                if (_preQueue.Count > 0)
                {
                    for (var i = 0; i < _preQueue.Count; i++)
                    {
                        InternalEnqueue(_preQueue[i]);
                    }
                }
            }

            return this;
        }

        public void Stop()
        {
            if (_state != ExecutionState.Running) return;
            lock (_preQueue)
            {
                _state = ExecutionState.Created;
            }
        }

        public void Enqueue(Func<Task> action)
        {
            if (_state == ExecutionState.Stopped)
                return;
            if (_state == ExecutionState.Created)
            {
                lock (_preQueue)
                {
                    if (_state == ExecutionState.Created)
                    {
                        _preQueue.Add(action);
                        return;
                    }
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

        public override void Dispose()
        {
            _state = ExecutionState.Stopped;
            base.Dispose();
        }

        protected virtual void InternalStart()
        {
        }

        protected abstract void InternalEnqueue(Func<Task> action);

    }
}