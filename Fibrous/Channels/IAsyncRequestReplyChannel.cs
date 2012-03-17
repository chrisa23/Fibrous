namespace Fibrous.Channels
{
    using System;

    /// <summary>
    /// Can receive more than one reply
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IAsyncRequestReplyChannel<TRequest, TReply> : IAsyncRequestPort<TRequest, TReply>
    {
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }
}