namespace Fibrous.Fibers.Queues
{
    public interface IWaitStrategy
    {
        void Wait();
        void SignalWhenBlocking();
    }
}