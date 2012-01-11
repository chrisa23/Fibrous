using System;
using System.Collections.Generic;
using System.Threading;
using Fibrous.Internal;

namespace Fibrous.Fibers.ThreadPool
{
    /// <summary>
    /// Fiber that uses a thread pool for execution.
    /// </summary>
    public sealed class PoolFiber : FiberBase
    {
        private enum ExecutionState
        {
            Created,
            Running,
            Stopped
        }

        private readonly object _lock = new object();
        private readonly IThreadPool _pool;
        private readonly IExecutor _executor;

        private List<Action> _queue = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        private ExecutionState _started = ExecutionState.Created;
        private bool _flushPending;


        public PoolFiber(IThreadPool pool, IExecutor executor)
        {
            _pool = pool;
            _executor = executor;
        }

        public PoolFiber(IExecutor executor)
            : this(new DefaultThreadPool(), executor)
        {
        }

        public PoolFiber()
            : this(new DefaultThreadPool(), new DefaultExecutor())
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
                    _pool.Queue(Flush);
                    _flushPending = true;
                }
            }
        }

        private void Flush(object state)
        {
            IEnumerable<Action> toExecute = ClearActions();
            if (toExecute != null)
            {
                _executor.Execute(toExecute);
                lock (_lock)
                {
                    if (_queue.Count > 0)
                    {
                        // don't monopolize thread.
                        _pool.Queue(Flush);
                    }
                    else
                    {
                        _flushPending = false;
                    }
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

        public override void Dispose()
        {
            _started = ExecutionState.Stopped;
            base.Dispose();
        }
    }
}