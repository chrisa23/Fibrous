using System;
using System.Collections.Generic;
using System.Threading;

namespace Fibrous.Fibers.Queues
{
    public abstract class QueueBase : IQueue
    {
        protected readonly object SyncRoot = new object();
        private readonly IExecutor _executor;
        protected List<Action> Actions = new List<Action>();
        protected volatile bool Running = true;
        protected List<Action> ToPass = new List<Action>();

        protected QueueBase(IExecutor executor)
        {
            _executor = executor;
        }

        protected QueueBase()
            : this(new DefaultExecutor())
        {
        }

        #region IQueue Members

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

        #endregion

        protected abstract IEnumerable<Action> DequeueAll();

        private bool ExecuteNextBatch()
        {
            IEnumerable<Action> toExecute = DequeueAll();
            if (toExecute == null)
                return false;
            _executor.Execute(toExecute);
            return true;
        }
    }
}