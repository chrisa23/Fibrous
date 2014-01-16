namespace Fibrous
{
    using System;

    public interface IRequestChannel<T, T1> : IRequestPort<T, T1>, IRequestHandlerPort<T, T1>
    {

    }

    public interface IReply<T>
    {
        Result<T> Receive(TimeSpan timeout);
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
    
    public interface IRequestPort<in TRequest, TReply>
    {
        IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply); //can this be an extension method?
        IReply<TReply> SendRequest(TRequest request);
    }
}