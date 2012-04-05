namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotSenderPort<in T, in TSnapshot> : ISenderPort<T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
    }

    public interface ISnapshotChannel<T, TSnapshot> : ISnapshotSenderPort<T, TSnapshot>
    {
        IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }
}