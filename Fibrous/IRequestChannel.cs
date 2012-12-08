namespace Fibrous
{
    public interface IRequestChannel<T, T1> : IRequestPort<T, T1>, IRequestHandlerPort<T, T1>
    {
    }
}