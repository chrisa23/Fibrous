namespace Fibrous
{
    using System;

    public interface ISnapshotPort<T>
    {
        IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<T[]> receiveSnapshot);
    }
}