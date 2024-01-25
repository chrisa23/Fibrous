/*
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Fiber that uses a thread pool for execution. Pool is used instead of thread, but messages are handled sequentially.
    /// </summary>
    public sealed class PoolFiber_OLD : FiberBase_old
    {
        private readonly object _lock = new();
        private readonly TaskFactory _taskFactory;
        private bool _flushPending;


        //TODO: make initial list size adjustable...
        private List<Action> _queue = new(1024 * 4);
        private List<Action> _toPass = new(1024 * 4);

        public PoolFiber_OLD(IExecutor config, TaskFactory taskFactory)
            : base(config) =>
            _taskFactory = taskFactory;

        public PoolFiber_OLD(IExecutor executor)
            : this(executor, Task.Factory)
        {
        }

        public PoolFiber_OLD(TaskFactory taskFactory)
            : this(new Executor(), taskFactory)
        {
        }

        public PoolFiber_OLD()
            : this(new Executor(), Task.Factory)
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
                for (int i = 0; i < toExecute.Count; i++)
                {
                    Executor.Execute(toExecute[i]);
                }

                lock (_lock)
                {
                    if (_queue.Count > 0)
                        // don't monopolize thread.
                    {
                        _taskFactory.StartNew(Flush);
                    }
                    else
                    {
                        _flushPending = false;
                    }
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
            PoolFiber_OLD f = new();
            f.Start();
            return f;
        }

        public static IFiber StartNew(IExecutor exec)
        {
            PoolFiber_OLD f = new(exec);
            f.Start();
            return f;
        }
    }
}
*/
