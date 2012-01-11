using System.Threading;

namespace Fibrous.Fibers.ThreadPool
{
    public interface IThreadPool
    {
        void Queue(WaitCallback callback);
    }
}