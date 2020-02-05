using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous
{
    public class Fiber : FiberBase
    {
        private readonly IQueue<Action> _queue;
        private readonly TaskFactory _taskFactory;
        private bool _flushPending;
        private SpinLock _spinLock = new SpinLock(false);

        public Fiber(IExecutor executor = null, int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null, IFiberScheduler scheduler = null)
            : base(executor, scheduler)
        {
            _queue = new ArrayQueue<Action>(size);
            _taskFactory = taskFactory ?? new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
        }

        public Fiber(Action<Exception> errorCallback, int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null, IFiberScheduler scheduler = null)
            : this(new ExceptionHandlingExecutor(errorCallback), size,  taskFactory, scheduler)
        {
        }

        protected override void InternalEnqueue(Action action)
        {
            var spinWait = default(AggressiveSpinWait);
            while (_queue.IsFull) spinWait.SpinOnce();

            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                _queue.Enqueue(action);

                if (_flushPending) return;

                _flushPending = true;
                _taskFactory.StartNew(Flush);
            }
            finally
            {
                if (lockTaken) _spinLock.Exit(false);
            }
        }

        private void Flush()
        {
            var (count, actions) = Drain();

            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var action = actions[i];
                    Executor.Execute(action);
                }
            }

            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (_queue.Count > 0)
                    //don't monopolize thread.
                    _taskFactory.StartNew(Flush);
                else
                    _flushPending = false;
            }
            finally
            {
                if (lockTaken) _spinLock.Exit(false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int, Action[]) Drain()
        {
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                var drain = _queue.Drain();

                if (drain.Item1 <= 0)
                    _flushPending = false;

                return drain;
            }
            finally
            {
                if (lockTaken) _spinLock.Exit(false);
            }
        }
    }

    [Obsolete]
    public sealed class PoolFiber:Fiber
    { }
}