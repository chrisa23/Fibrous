using System;

namespace Fibrous
{
    public interface ISnapshotChannel<T, TSnapshot> : IPublisherPort<T>, ISnapshotSubscriberPort<T, TSnapshot>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
    }

    public interface ISnapshotSubscriberPort<out T, out TSnapshot>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }
}
