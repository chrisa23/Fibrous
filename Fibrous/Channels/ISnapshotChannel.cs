using System;

namespace Fibrous.Channels
{
    public interface ISnapshotPublisherPort<in T, in TSnapshot> : IPublisherPort<T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
    }

    public interface ISnapshotChannel<T, TSnapshot> : ISnapshotPublisherPort<T, TSnapshot>
    {
        IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }
}