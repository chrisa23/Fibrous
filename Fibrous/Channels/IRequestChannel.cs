namespace Fibrous.Channels
{
    using System;

    public interface IRequestChannel<TRequest, TReply> : IRequestPort<TRequest, TReply>
    {
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }
}