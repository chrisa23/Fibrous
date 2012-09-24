namespace Fibrous
{
    using System;

    public static class RequestPortExtensions
    {
        public static TReply SendRequest<TRequest, TReply>(this IRequestPort<TRequest, TReply> port, TRequest request)
        {
            return port.SendRequest(request, TimeSpan.MaxValue);
        }
    }
}