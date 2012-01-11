using System;

namespace Fibrous.Channels
{
    public interface IRequestReplyChannel<TRequest, TReply>
        : IRequestPort<TRequest, TReply>
    {
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }
}