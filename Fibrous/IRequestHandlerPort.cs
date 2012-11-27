namespace Fibrous
{
    using System;

    public interface IRequestHandlerPort<out TRequest, in TReply>
    {
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }
}