namespace Fibrous.Fibers
{
    using System;

    public interface IQueue
    {
        void Enqueue(Action action);
        void Run();
        void Stop();
    }
}