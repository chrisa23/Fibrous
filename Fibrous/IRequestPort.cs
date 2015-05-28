using System;

namespace Fibrous
{
    public interface IRequestPort<in TRequest, TReply>
    {
        IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply); 
        IReply<TReply> SendRequest(TRequest request);
    }

    public interface IResult<T>
    {
         bool IsValid { get; }
         T Value { get; }
    }

    public interface IReply<T>
    {
        IResult<T> Receive(TimeSpan timeout);
    }

    public interface IRequest<out TRequest, in TReply>
    {
        TRequest Request { get; }
        void Reply(TReply reply);
    }

    public interface IRequestHandlerPort<out TRequest, in TReply>
    {
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }

}
