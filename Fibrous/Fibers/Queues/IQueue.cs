using System;

namespace Fibrous.Fibers.Queues
{
    //TODO:  Remove and switch to simplified disruptor fiber
    public interface IQueue
    {
        void Enqueue(Action action);
        void Run();
        void Stop();
    }
}