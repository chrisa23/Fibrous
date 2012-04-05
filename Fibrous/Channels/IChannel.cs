namespace Fibrous.Channels
{
    public interface IChannel<T> : ISenderPort<T>, ISubscriberPort<T>
    {
    }
}