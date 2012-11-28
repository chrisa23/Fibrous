namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections.Generic;

    public interface IQueue : IDisposable
    {
        void Enqueue(Action action);
        IEnumerable<Action> DequeueAll();
    }
}