namespace Fibrous
{
    using System;
    using Fibrous.Channels;

    public interface IRequestHandlerPort<out TRequest, in TReply>
    {
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }
}