namespace Fibrous.Experimental
{
    using System;

    public interface ISnapshotPublishPort<in T> : IPublishPort<T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T[]> reply);
    }
}