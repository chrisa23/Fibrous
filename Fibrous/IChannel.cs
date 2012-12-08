namespace Fibrous
{
    public interface IChannel<T> : IPublisherPort<T>, ISubscriberPort<T>
    {
    }
}