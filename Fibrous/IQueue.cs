namespace Fibrous
{
    using System;

    public interface IQueue : IDisposable
    {
        void Enqueue(Action action);
        void Drain(Executor executor);
    }
}