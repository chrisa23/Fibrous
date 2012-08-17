namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotPublishPort<in T, in TSnapshot> : IPublishPort<T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
    }

    public interface ISnapshotChannel<T, TSnapshot> : ISnapshotPublishPort<T, TSnapshot>
    {
        IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }
}