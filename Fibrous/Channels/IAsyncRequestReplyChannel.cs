namespace Fibrous.Channels
{
    using System;

    /// <summary>
    /// Asynchronous ReqReply channel
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IAsyncRequestReplyChannel<TRequest, TReply> : IAsyncRequestPort<TRequest, TReply>
    {
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }
}