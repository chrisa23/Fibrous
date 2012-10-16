using System;
using Fibrous.Channels;

namespace Fibrous
{
    public interface IRequestHandlerPort<TRequest, TReply>
    {
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }
}