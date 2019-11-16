using System;

namespace Fibrous
{
    public interface ISnapshotPublisherPort<in T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T> reply);
    }
}