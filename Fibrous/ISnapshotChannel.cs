using System;
using System.Threading.Tasks;

namespace Fibrous;

public interface ISnapshotChannel<T, TSnapshot> : IPublisherPort<T>, ISnapshotSubscriberPort<T, TSnapshot>,
    IDisposable
{
    IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
    IDisposable ReplyToPrimingRequest(IAsyncFiber fiber, Func<Task<TSnapshot>> reply);
}

public interface ISnapshotSubscriberPort<out T, out TSnapshot>
{
    IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);

    IDisposable Subscribe(IAsyncFiber fiber, Func<T, Task> receive, Func<TSnapshot, Task> receiveSnapshot);
}
