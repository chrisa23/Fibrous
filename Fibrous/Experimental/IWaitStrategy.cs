namespace Fibrous.Experimental
{
    public interface IWaitStrategy
    {
        void Wait();
        void SignalWhenBlocking();
    }
}