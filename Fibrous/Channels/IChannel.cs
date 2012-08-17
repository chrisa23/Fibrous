namespace Fibrous.Channels
{
    public interface IChannel<T> : IPublishPort<T>, ISubscribePort<T>
    {
    }
}