using System;
using System.Threading;
using System.Threading.Tasks;
using Fibrous.Internal;

namespace Fibrous
{
    public class PoolFiber : FiberBase
    {
        private readonly ArrayQueue<Action> _queue;

        private readonly TaskFactory _taskFactory =
            new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);

        private bool _flushPending;
        private SpinLock _spinLock = new SpinLock(false);

        public PoolFiber(IExecutor config, int size = 1024 * 16)
            : base(config)
        {
            _queue = new ArrayQueue<Action>(size);
        }

        public PoolFiber() : this(new Executor())
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
                if (!_flushPending)
                {
                    _taskFactory.StartNew(Flush);
                    _flushPending = true;
                }
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
                for (var i = 0; i < count; i++) Executor.Execute(actions[i]);

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
        }

        private (int, Action[]) Drain()
        {
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (_queue.Count == 0)
                {
                    _flushPending = false;
                    return ArrayQueue<Action>.Empty;
                }

                return _queue.Drain();
            }
            finally
            {
                if (lockTaken) _spinLock.Exit(false);
            }
        }

        public static IFiber StartNew()
        {
            var fiber = new PoolFiber();
            fiber.Start();
            return fiber;
        }

        public static IFiber StartNew(IExecutor exec = null, int size = 1024 * 16)
        {
            var fiber = new PoolFiber(exec ?? new Executor(), size);
            fiber.Start();
            return fiber;
        }
    }
}