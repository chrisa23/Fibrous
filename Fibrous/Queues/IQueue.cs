namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    public interface IQueue : IDisposable
    {
        void Enqueue(Action action);
        List<Action> Drain();
    }
}