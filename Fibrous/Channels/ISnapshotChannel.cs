namespace Fibrous.Channels
{
    public interface ISnapshotChannel<T> : ISnapshotPublishPort<T>, ISnapshotPort<T>
    {
    }
}