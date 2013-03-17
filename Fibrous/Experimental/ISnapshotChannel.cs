namespace Fibrous.Experimental
{
    public interface ISnapshotChannel<T> : ISnapshotPublisherPort<T>,ISubscriberPort<T>
    {
        //I have never used this yet, but what does it imply
        //a collection being observed after starting...
        //Should I e
    }
}