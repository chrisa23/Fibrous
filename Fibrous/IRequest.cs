namespace Fibrous
{
    public interface IRequest<out TRequest, in TReply>
    {
        TRequest Request { get; }
        void Reply(TReply reply);
    }
}