namespace Fibrous.Fibers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public abstract class GuiFiberBase : FiberBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly object _lock = new object();
        private readonly List<Action> _queue = new List<Action>();
        private volatile ExecutionState _started = ExecutionState.Created;

        protected GuiFiberBase(FiberConfig config, IExecutionContext executionContext) : base(config)
        {
            _executionContext = executionContext;
        }

        public override void Enqueue(Action action)
        {
            if (_started == ExecutionState.Stopped)
                return;
            if (_started == ExecutionState.Created)
            {
                lock (_lock)
                {
                    if (_started == ExecutionState.Created)
                    {
                        _queue.Add(action);
                        return;
                    }
                }
            }
            _executionContext.Enqueue(() => Executor.Execute(action));
        }

        public override void Start()
        {
            if (_started == ExecutionState.Running)
                throw new ThreadStateException("Already Started");
            lock (_lock)
            {
                Action[] actions = _queue.ToArray();
                _queue.Clear();
                if (actions.Length > 0)
                    _executionContext.Enqueue(() => Executor.Execute(actions));
                _started = ExecutionState.Running;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _started = ExecutionState.Stopped;
            base.Dispose(disposing);
        }
    }
}