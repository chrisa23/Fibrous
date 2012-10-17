namespace Fibrous
{
    using System;

    public interface ISnapshotPort<T, TSnapshot>
    {
        IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot, TimeSpan timeout);
    }
}