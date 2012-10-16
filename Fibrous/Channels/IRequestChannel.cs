namespace Fibrous.Channels
{
    public interface IRequestChannel<TRequest, TReply> : IRequestPort<TRequest, TReply>,
                                                         IRequestHandlerPort<TRequest, TReply>
    {
    }
}