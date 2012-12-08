namespace Fibrous.Experimental
{
    public interface ISnapshotChannel<T> : ISnapshotPublisherPort<T>, ISnapshotPort<T>
    {
    }
}