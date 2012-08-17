namespace Fibrous.Channels
{
    public interface IRequest<out TRequest, in TReply>
    {
        TRequest Request { get; }
        bool Reply(TReply reply);
    }
}