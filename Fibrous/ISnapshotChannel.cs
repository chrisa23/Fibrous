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
    
    //TODO:  think about adjusting this...
    public interface ISnapshotPublisherPort<in TSnapshot>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
    }
}
