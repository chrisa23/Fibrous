namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public abstract class QueueBase : IQueue
    {
        protected readonly object SyncRoot = new object();
        private readonly IExecutor _executor;
        protected volatile bool Running = true;
        protected List<Action> Actions = new List<Action>();
        protected List<Action> ToPass = new List<Action>();

        protected QueueBase(IExecutor executor)
        {
            _executor = executor;
        }

        protected QueueBase()
            : this(new DefaultExecutor())
        {
        }

        public virtual void Enqueue(Action action)
        {
            lock (SyncRoot)
            {
                Actions.Add(action);
                Monitor.PulseAll(SyncRoot);
            }
        }

        public void Run()
        {
            while (ExecuteNextBatch())
            {
            }
        }

        public void Stop()
        {
            lock (SyncRoot)
            {
                Running = false;
                Monitor.PulseAll(SyncRoot);
            }
        }

        protected abstract IEnumerable<Action> DequeueAll();

        private bool ExecuteNextBatch()
        {
            IEnumerable<Action> toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _executor.Execute(toExecute);
            return true;
        }
    }
}