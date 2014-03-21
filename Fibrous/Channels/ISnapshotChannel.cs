namespace Fibrous.Channels
{
    using System;

    public interface ISnapshotChannel<T> : ISnapshotPublisherPort<T>//,ISnapshotSubscriberPort<T>
    {
        //I have never used this yet, but what does it imply
        //a collection being observed after starting...
        //Should I e
        IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<T[]> receiveSnapshot);
    }
}