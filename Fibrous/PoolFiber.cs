namespace Fibrous
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fibrous.Experimental;
    using Fibrous.Queues;

    /// <summary>
    /// Fiber that uses a thread pool for execution.
    /// </summary>
    public sealed class PoolFiber : Fiber
    {
        private readonly object _lock = new object();
        private readonly TaskFactory _taskFactory;
        private bool _flushPending;
        private List<Action> _queue = new List<Action>();
        private List<Action> _toPass = new List<Action>();
        
        public PoolFiber(Executor config, TaskFactory taskFactory) : base(config)
        {
            _taskFactory = taskFactory;
        }

        public PoolFiber(Executor executor)
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
            IEnumerable<Action> toExecute = ClearActions();
            if (toExecute != null)
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

        private IEnumerable<Action> ClearActions()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    _flushPending = false;
                    return null;
                }
                Lists.Swap(ref _queue, ref _toPass);
                _queue.Clear();
                return _toPass;
            }
        }

        public static Fiber StartNew()
        {
            var fiber = new PoolFiber();
            fiber.Start();
            return fiber;
        }

        public static Fiber StartNew(Executor exec)
        {
            var fiber = new PoolFiber(exec);
            fiber.Start();
            return fiber;
        }
    }
}