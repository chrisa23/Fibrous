namespace Fibrous.Experimental
{
    using System;

    public interface ISnapshotPort<T>
    {
        IDisposable PrimedSubscribe(Fiber fiber, Action<T> receive, Action<T[]> receiveSnapshot);
    }
}