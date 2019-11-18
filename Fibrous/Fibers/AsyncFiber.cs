using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous
{
    public class AsyncFiber : AsyncFiberBase
    {
        private readonly ArrayQueue<Func<Task>> _queue;

        private readonly TaskFactory _taskFactory =
            new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);

        private bool _flushPending;
        private SpinLock _spinLock = new SpinLock(false);

        public AsyncFiber(IAsyncExecutor executor, int size = QueueSize.DefaultQueueSize)
            : base(executor)
        {
            _queue = new ArrayQueue<Func<Task>>(size);
        }

        public AsyncFiber() : this(new AsyncExecutor())
        {
        }

        public AsyncFiber(IAsyncExecutor executor, int queueSize, IAsyncFiberScheduler scheduler):base(executor, scheduler)
        {
            _queue = new ArrayQueue<Func<Task>>(queueSize);
        }

        protected override void InternalEnqueue(Func<Task> action)
        {
            var spinWait = default(AggressiveSpinWait);
            while (_queue.IsFull) spinWait.SpinOnce();

            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                _queue.Enqueue(action);
                if (!_flushPending)
                {
                    _taskFactory.StartNew(Flush);
                    _flushPending = true;
                }
            }
            finally
            {
                if (lockTaken) _spinLock.Exit();
            }
        }

        private async Task Flush()
        {
            var (count, actions) = Drain();
            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var action = actions[i];
                    await Executor.Execute(action);
                }

                var lockTaken = false;
                try
                {
                    _spinLock.Enter(ref lockTaken);

                    if (_queue.Count > 0)
                        //don't monopolize thread.
#pragma warning disable 4014
                        _taskFactory.StartNew(Flush);
#pragma warning restore 4014
                    else
                        _flushPending = false;
                }
                finally
                {
                    if (lockTaken) _spinLock.Exit();
                }
            }
        }

        private (int, Func<Task>[]) Drain()
        {
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (_queue.Count == 0)
                {
                    _flushPending = false;
                    return ArrayQueue<Func<Task>>.Empty;
                }

                return _queue.Drain();
            }
            finally
            {
                if (lockTaken) _spinLock.Exit();
            }
        }

        public static IAsyncFiber StartNew()
        {
            var fiber = new AsyncFiber();
            fiber.Start();
            return fiber;
        }

        public static IAsyncFiber StartNew(IAsyncExecutor exec)
        {
            var fiber = new AsyncFiber(exec);
            fiber.Start();
            return fiber;
        }


        public static IAsyncFiber StartNew(IAsyncExecutor exec, int size, IAsyncFiberScheduler scheduler = null)
        {
            var fiber = new AsyncFiber(exec, size, scheduler ?? new AsyncTimerScheduler());
            fiber.Start();
            return fiber;
        }
    }
}