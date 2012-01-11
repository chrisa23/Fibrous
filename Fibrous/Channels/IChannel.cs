namespace Fibrous.Channels
{
    public interface IChannel<T> : IPublisherPort<T>, ISubscriberPort<T>
    {
    }
}