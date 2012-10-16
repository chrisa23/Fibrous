namespace Fibrous.Channels
{
    public interface IAsyncSnapshotChannel<T, TSnapshot> : ISnapshotPublishPort<T, TSnapshot>,
                                                           IAsyncSnapshotPort<T, TSnapshot>
    {
    }
}