namespace Fibrous.Experimental
{
    using System;

    public interface ISnapshotPublisherPort<in T> : IPublisherPort<T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T[]> reply);
    }
}