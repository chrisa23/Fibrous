using System;
using System.Threading.Tasks;

namespace Fibrous;

public interface ISnapshotChannel<T, TSnapshot> : IPublisherPort<T>, ISnapshotSubscriberPort<T, TSnapshot>,
    IDisposable
{
    IDisposable ReplyToPrimingRequest(IAsyncFiber fiber, Func<Task<TSnapshot>> reply);
}

public interface ISnapshotSubscriberPort<out T, out TSnapshot>
{
    IDisposable Subscribe(IAsyncFiber fiber, Func<T, Task> receive, Func<TSnapshot, Task> receiveSnapshot);
}
