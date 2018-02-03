namespace Fibrous
{
    using System;

    public static class RequestPortExtensions
    {
        /// <summary>
        /// Send a request with infinite timeout
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TReply"></typeparam>
        /// <param name="port"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static TReply SendRequest<TRequest, TReply>(this IRequestPort<TRequest, TReply> port, TRequest request)
        {
            return port.SendRequest(request).Receive(TimeSpan.MaxValue).Value;
        }
    }
}