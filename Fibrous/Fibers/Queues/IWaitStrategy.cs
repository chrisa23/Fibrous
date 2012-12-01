namespace Fibrous.Fibers.Queues
{
    using System.Threading;

    public interface IWaitStrategy
    {
        void Wait();
        void SignalWhenBlocking();
    }

    public class YieldingWaitStrategy : IWaitStrategy
    {
        public void Wait()
        {
            Thread.Yield();
        }

        public void SignalWhenBlocking()
        {
        
        }
    }

    public sealed class BlockingWait : IWaitStrategy
    {
        private readonly object _syncRoot = new object();

        public void SignalWhenBlocking()
        {
            lock (_syncRoot)
            {
                Monitor.PulseAll(_syncRoot);
            }
        }

        public void Wait()
        {
            lock (_syncRoot)
            {
                    Monitor.Wait(_syncRoot);
            }
        }

    }
}