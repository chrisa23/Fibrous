namespace Fibrous
{
    public interface IPublisherPort<in T>
    {
        bool Publish(T msg);
    }
}