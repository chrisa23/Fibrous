namespace Fibrous.Fibers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fibrous.Fibers.Queues;

    /// <summary>
    /// Fiber that uses a thread pool for execution.
    /// </summary>
    public sealed class PoolFiber : FiberBase
    {
        //private readonly IExecutor _executor;
        private readonly object _lock = new object();
        private readonly TaskFactory _taskFactory;
        private bool _flushPending;
        private ExecutionState _started = ExecutionState.Created;
        private List<Action> _queue = new List<Action>();
        private List<Action> _toPass = new List<Action>();
        //switch back to IPool 
        public PoolFiber(FiberConfig config, TaskFactory taskFactory) : base(config)
        {
            _taskFactory = taskFactory;
        }

        public PoolFiber(IExecutor executor)
            : this(new FiberConfig(executor), new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None))
        {
        }

        public PoolFiber(IExecutor executor, TaskFactory taskFactory)
            : this(new FiberConfig(executor), taskFactory)
        {
        }

        public PoolFiber(TaskFactory taskFactory)
            : this(FiberConfig.Default, taskFactory)
        {
        }

        public PoolFiber()
            : this(FiberConfig.Default, new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None))
        {
        }

        public override void Enqueue(Action action)
        {
            if (_started == ExecutionState.Stopped)
                return;
            lock (_lock)
            {
                _queue.Add(action);
                if (_started == ExecutionState.Created)
                    return;
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

        public override void Start()
        {
            if (_started == ExecutionState.Running)
                throw new ThreadStateException("Already Started");
            _started = ExecutionState.Running;
            //flush any pending events in queue
            Enqueue(() => { });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _started = ExecutionState.Stopped;
            base.Dispose(disposing);
        }

        #region StartNew

        public static IFiber StartNew()
        {
            var fiber = new PoolFiber();
            fiber.Start();
            return fiber;
        }

        public static IFiber StartNew(IExecutor executor)
        {
            var fiber = new PoolFiber(executor);
            fiber.Start();
            return fiber;
        }

        public static IFiber StartNew(TaskFactory taskFactory)
        {
            var fiber = new PoolFiber(taskFactory);
            fiber.Start();
            return fiber;
        }

        public static IFiber StartNew(IExecutor executor, TaskFactory taskFactory)
        {
            var fiber = new PoolFiber(executor, taskFactory);
            fiber.Start();
            return fiber;
        }

        #endregion
    }
}