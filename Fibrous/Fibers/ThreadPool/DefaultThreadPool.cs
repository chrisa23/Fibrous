using System.Threading;

namespace Fibrous.Fibers.ThreadPool
{
    public sealed class DefaultThreadPool : IThreadPool
    {
        public void Queue(WaitCallback callback)
        {
            if (!System.Threading.ThreadPool.QueueUserWorkItem(callback))
            {
                throw new QueueFullException("Unable to add item to pool: " + callback.Target);
            }
        }
    }
}