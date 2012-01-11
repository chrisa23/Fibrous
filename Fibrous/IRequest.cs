namespace Fibrous
{
    public interface IRequest<out TRequest, in TReply>
        : IPublisherPort<TReply>
    {
        TRequest Request { get; }
    }
}