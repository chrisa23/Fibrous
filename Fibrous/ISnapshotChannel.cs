using System;

namespace Fibrous
{
    public interface ISnapshotChannel<T, TSnapshot> : IPublisherPort<T>, ISnapshotPublisherPort<TSnapshot>,
        ISnapshotSubscriberPort<T, TSnapshot>
    {
    }

    public interface ISnapshotSubscriberPort<out T, out TSnapshot>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }

    public interface ISnapshotPublisherPort<in T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T> reply);
    }
}