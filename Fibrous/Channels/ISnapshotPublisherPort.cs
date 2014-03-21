namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotPublisherPort<in T> : IPublisherPort<T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T[]> reply);
    }
}