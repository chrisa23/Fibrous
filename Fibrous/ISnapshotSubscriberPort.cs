using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface ISnapshotSubscriberPort<out T, out TSnapshot>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);

        IDisposable Subscribe(IAsyncFiber fiber, Func<T, Task> receive, Func<TSnapshot, Task> receiveSnapshot);
    }
}
