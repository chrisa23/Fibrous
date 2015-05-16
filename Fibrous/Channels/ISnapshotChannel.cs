namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotChannel<T> : ISnapshotPublisherPort<T>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<T[]> receiveSnapshot);
    }
}