namespace Fibrous.Fibers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fibrous.Queues;

    /// <summary>
    /// Fiber that uses a thread pool for execution. Pool is used instead of thread, but messages are handled sequentially. 
    /// </summary>
    public sealed class PoolFiber : FiberBase
    {
        private readonly object _lock = new object();
        private readonly TaskFactory _taskFactory;
        private bool _flushPending;
        private List<Action> _queue = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        public PoolFiber(IExecutor config, TaskFactory taskFactory)
            : base(config)
        {
            _taskFactory = taskFactory;
        }

        public PoolFiber(IExecutor executor)
            : this(executor, new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None))
        {
        }

        public PoolFiber(TaskFactory taskFactory)
            : this(new Executor(), taskFactory)
        {
        }

        public PoolFiber()
            : this(new Executor(), new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None))
        {
        }

        protected override void InternalEnqueue(Action action)
        {
            lock (_lock)
            {
                _queue.Add(action);
                if (!_flushPending)
                {
                    _taskFactory.StartNew(Flush);
                    _flushPending = true;
                }
            }
        }

        private void Flush()
        {
            List<Action> toExecute = ClearActions();
            if (toExecute.Count > 0)
            {
                Executor.Execute(toExecute);
                lock (_lock)
                {
                    if (_queue.Count > 0)
                    {
                        // don't monopolize thread.
                        _taskFactory.StartNew(Flush);
                    }
                    else
                        _flushPending = false;
                }
            }
        }

        private List<Action> ClearActions()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    _flushPending = false;
                    return Queue.Empty;
                }
                Lists.Swap(ref _queue, ref _toPass);
                _queue.Clear();
                return _toPass;
            }
        }

        public static IFiber StartNew()
        {
            return Fiber.StartNew(FiberType.Pool);
        }

        public static IFiber StartNew(IExecutor exec)
        {
            return Fiber.StartNew(FiberType.Pool, exec);
        }
    }
}