namespace Fibrous.Channels
{
    public interface ISnapshotChannel<T, TSnapshot> : ISnapshotPublishPort<T, TSnapshot>,
                                                      ISnapshotPort<T, TSnapshot>
    {
    }
}