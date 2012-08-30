namespace Fibrous
{
    using System;

    public interface IRequestPort<in TRequest, out TReply>
    {
        TReply SendRequest(TRequest request, TimeSpan timeout);
    }

    public static class RequestPortExtensions
    {
         public static TReply SendRequest<TRequest,TReply>(this IRequestPort<TRequest,TReply> port, TRequest request)
         {
             return port.SendRequest(request, TimeSpan.MaxValue);
         }
    }
}