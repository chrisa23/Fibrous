namespace Fibrous.Queues
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Queue for fibers.  Drain always grabs full batch.
    /// </summary>
    public interface IQueue : IDisposable
    {
        void Enqueue(Action action);
        List<Action> Drain();
    }
}