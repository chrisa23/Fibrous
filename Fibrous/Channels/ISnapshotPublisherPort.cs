namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotPublisherPort<in T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T> reply);
    }
}