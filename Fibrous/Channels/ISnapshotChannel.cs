using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Channel that combines incremental updates with snapshot subscription support.
/// </summary>
public interface ISnapshotChannel<T, TSnapshot> :
    IPublisherPort<T>,
    ISnapshotSubscriberPort<T, TSnapshot>,
    IDisposable
{
    IDisposable ReplyToPrimingRequest(IFiber fiber, Func<Task<TSnapshot>> reply);
}

/// <summary>
///     Subscriber port that delivers an initial snapshot and then later incremental updates.
/// </summary>
public interface ISnapshotSubscriberPort<out T, out TSnapshot>
{
    IDisposable Subscribe(IFiber fiber, Func<T, Task> receive, Func<TSnapshot, Task> receiveSnapshot);
}
