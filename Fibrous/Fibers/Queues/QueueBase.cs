namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections.Generic;

    public abstract class QueueBase : IQueue
    {
        protected List<Action> Actions = new List<Action>();
        protected List<Action> ToPass = new List<Action>();

        public virtual void Enqueue(Action action)
        {
            Actions.Add(action);
        }

        public abstract void Drain(IExecutor executor);

        //public abstract IEnumerable<Action> DequeueAll();

        public virtual void Dispose()
        {
        }
    }
}