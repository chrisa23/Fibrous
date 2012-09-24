namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotPublishPort<in T, in TSnapshot> : IPublishPort<T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
    }
}