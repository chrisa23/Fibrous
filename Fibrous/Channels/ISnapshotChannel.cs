namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotChannel<T, TSnapshot> : IPublisherPort<T>, ISnapshotPublisherPort<TSnapshot>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }
}