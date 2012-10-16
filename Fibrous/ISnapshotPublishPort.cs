using System;

namespace Fibrous
{
    public interface ISnapshotPublishPort<in T, in TSnapshot> : IPublishPort<T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
    }
}