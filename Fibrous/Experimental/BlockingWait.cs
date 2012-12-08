namespace Fibrous.Experimental
{
    using System.Threading;

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