namespace Fibrous.Experimental
{
    using System.Threading;

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
}