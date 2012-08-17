namespace Fibrous
{
    public interface IPublishPort<in T>
    {
        bool Publish(T msg);
    }
}