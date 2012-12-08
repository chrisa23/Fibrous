namespace Fibrous
{
    using System;

    public interface IRequestHandlerPort<out TRequest, in TReply>
    {
        IDisposable SetRequestHandler(Fiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }
}