namespace Fibrous.Channels
{
    /// <summary>
    /// IChannels are in-memory conduits for messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannel<T> : IPublisherPort<T>, ISubscriberPort<T>
    {
    }
}