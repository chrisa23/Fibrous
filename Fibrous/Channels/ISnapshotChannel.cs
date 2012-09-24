namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotChannel<T, TSnapshot> : ISnapshotPublishPort<T, TSnapshot>
    {
        IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot, TimeSpan timeout);
    }
}