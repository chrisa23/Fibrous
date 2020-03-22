using System;

namespace Fibrous
{
    public interface ISnapshotChannel<T, TSnapshot> : IPublisherPort<T>, ISnapshotSubscriberPort<T, TSnapshot>, IDisposable
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
    }
}
