using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous
{
    public abstract class AsyncFiberBase : Disposables, IAsyncFiber
    {
        private readonly IAsyncFiberScheduler _fiberScheduler;
        protected readonly IAsyncExecutor Executor;
        private bool _disposed;

        protected AsyncFiberBase(IAsyncExecutor executor = null, IAsyncFiberScheduler scheduler = null)
        {
            _fiberScheduler = scheduler ?? new AsyncTimerScheduler();
            Executor = executor ?? new AsyncExecutor();
        }

        public IDisposable Schedule(Func<Task> action, TimeSpan dueTime)
        {
            return _fiberScheduler.Schedule(this, action, dueTime);
        }

        public IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval)
        {
            return _fiberScheduler.Schedule(this, action, startTime, interval);
        }

        public void Enqueue(Func<Task> action)
        {
            if (_disposed) return;

            InternalEnqueue(action);
        }

        protected abstract void InternalEnqueue(Func<Task> action);
        
        public override void Dispose()
        {
            _disposed = true;
            base.Dispose();
        }
    }
}