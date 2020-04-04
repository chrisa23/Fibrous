using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous
{
    public class LockAsyncFiber : AsyncFiberBase
    {
        private readonly ArrayQueue<Func<Task>> _queue;
        private readonly TaskFactory _taskFactory;
        private readonly Func<Task> _flushCache;
        private bool _flushPending;
        private readonly object _lock = new object();

        public LockAsyncFiber(IAsyncExecutor executor = null, int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null, IAsyncFiberScheduler scheduler = null)
            : base(executor, scheduler)
        {

            _queue = new ArrayQueue<Func<Task>>(size);
            _taskFactory = taskFactory ?? new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
            _flushCache = Flush;
        }

        public LockAsyncFiber(Action<Exception> errorCallback, int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null, IAsyncFiberScheduler scheduler = null)
            : this(new AsyncExceptionHandlingExecutor(errorCallback), size, taskFactory, scheduler)
        {
        }

        protected override void InternalEnqueue(Func<Task> action)
        {
            var spinWait = default(AggressiveSpinWait);
            while (_queue.IsFull) spinWait.SpinOnce();
            lock (_lock) { 
                _queue.Enqueue(action);

                if (_flushPending) return;

                _flushPending = true;
                _taskFactory.StartNew(_flushCache);
            }
           
        }

        private async Task Flush()
        {
            var (count, actions) = Drain();

            for (var i = 0; i < count; i++)
            {
                var action = actions[i];
                await Executor.Execute(action);
            }

            lock (_lock)
            {

                if (_queue.Count > 0)
                    //don't monopolize thread.
#pragma warning disable 4014
                    _taskFactory.StartNew(_flushCache);
#pragma warning restore 4014
                else
                    _flushPending = false;
            }
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int, Func<Task>[]) Drain()
        {
            lock (_lock)
            {
                return _queue.Drain();
            }
        }
    }
}