using System;

namespace Fibrous.Fibers.Thread
{
    public interface IQueue
    {
        void Enqueue(Action action);
        void Run();
        void Stop();
    }
}