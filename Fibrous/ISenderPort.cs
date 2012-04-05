namespace Fibrous
{
    public interface ISenderPort<in T>
    {
        bool Send(T msg);
    }
}