namespace Fibrous
{
    using System;

    public interface IAsyncSnapshotPort<T, TSnapshot>
    {
        IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }
}