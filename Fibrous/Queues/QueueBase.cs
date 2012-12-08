namespace Fibrous.Queues
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

        public abstract void Drain(Executor executor);

        public virtual void Dispose()
        {
        }
    }
}