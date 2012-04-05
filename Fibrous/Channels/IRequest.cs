namespace Fibrous.Channels
{
    public interface IRequest<out TRequest, in TReply> : ISenderPort<TReply>
    {
        TRequest Request { get; }
    }
}