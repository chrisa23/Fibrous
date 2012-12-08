namespace Fibrous.Experimental
{
    using System;

    public interface ISnapshotPublisherPort<in T> : IPublisherPort<T>
    {
        IDisposable ReplyToPrimingRequest(Fiber fiber, Func<T[]> reply);
    }
}