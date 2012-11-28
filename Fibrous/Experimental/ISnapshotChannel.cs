namespace Fibrous.Experimental
{
    public interface ISnapshotChannel<T> : ISnapshotPublishPort<T>, ISnapshotPort<T>
    {
    }
}