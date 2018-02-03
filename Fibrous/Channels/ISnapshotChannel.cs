namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotChannel<T, TSnapshot> : IPublisherPort<T>, ISnapshotPublisherPort<TSnapshot>, ISnapshotSubscriberPort<T, TSnapshot>
    {
        
    }

    public interface ISnapshotSubscriberPort<out T, out TSnapshot>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }
}