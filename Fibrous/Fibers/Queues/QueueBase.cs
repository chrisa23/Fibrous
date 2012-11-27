namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public abstract class QueueBase : IQueue
    {
        protected static readonly Action[] Empty = new Action[0];
        protected readonly object SyncRoot = new object();
        protected List<Action> Actions = new List<Action>();
        protected List<Action> ToPass = new List<Action>();

        public virtual void Enqueue(Action action)
        {
            lock (SyncRoot)
            {
                Actions.Add(action);
                Monitor.PulseAll(SyncRoot);
            }
        }

        public bool HasItems()
        {
            lock (SyncRoot)
            {
                return Actions.Count > 0;
            }
        }

        public abstract IEnumerable<Action> DequeueAll();

        public void Dispose()
        {
            lock (SyncRoot)
            {
                Monitor.PulseAll(SyncRoot);
            }
        }
    }
}