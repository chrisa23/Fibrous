using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     It is suggested to always use an Exception callback with the IAsyncFiber
    /// </summary>
    public class AsyncFiber : AsyncFiberBase
    {
        private readonly Func<Task> _flushCache;
        private readonly ArrayQueue<Func<Task>> _queue;
        private readonly TaskFactory _taskFactory;
        private bool _flushPending;
        private SpinLock _spinLock = new SpinLock(false);

        public AsyncFiber(IAsyncExecutor executor = null, int size = QueueSize.DefaultQueueSize,
            TaskFactory taskFactory = null, IAsyncFiberScheduler scheduler = null)
            : base(executor, scheduler)
        {
            _queue = new ArrayQueue<Func<Task>>(size);
            _taskFactory = taskFactory ??
                           new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
            _flushCache = FlushAsync;
        }

        public AsyncFiber(Action<Exception> errorCallback, int size = QueueSize.DefaultQueueSize,
            TaskFactory taskFactory = null, IAsyncFiberScheduler scheduler = null)
            : this(new AsyncExceptionHandlingExecutor(errorCallback), size, taskFactory, scheduler)
        {
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void InternalEnqueue(Func<Task> action)
        {
            AggressiveSpinWait spinWait = default(AggressiveSpinWait);
            //SpinWait spinWait = new SpinWait();
            while (_queue.IsFull)
            {
                spinWait.SpinOnce();
            }

            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                _queue.Enqueue(action);

                if (_flushPending)
                {
                    return;
                }

                _flushPending = true;
                _ = _taskFactory.StartNew(_flushCache);
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit(false);
                }
            }
        }

        private async Task FlushAsync()
        {
            (int count, Func<Task>[] actions) = Drain();

            for (int i = 0; i < count; i++)
            {
                await Executor.ExecuteAsync(actions[i]);
            }

            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (_queue.Count > 0)
                {
                    _ = _taskFactory.StartNew(_flushCache);
                }
                else
                {
                    _flushPending = false;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit(false);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int, Func<Task>[]) Drain()
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                return _queue.Drain();
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit(false);
                }
            }
        }
    }
}
