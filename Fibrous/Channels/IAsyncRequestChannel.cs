namespace Fibrous.Channels
{
    /// <summary>
    /// Asynchronous ReqReply channel
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IAsyncRequestChannel<TRequest, TReply> : IAsyncRequestPort<TRequest, TReply>,
                                                              IRequestHandlerPort<TRequest, TReply>
    {
    }
}