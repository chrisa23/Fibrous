using System;

namespace Fibrous
{
    public interface ISnapshotSubscriberPort<out T, out TSnapshot>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }
}