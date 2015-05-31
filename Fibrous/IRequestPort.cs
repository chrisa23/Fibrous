namespace Fibrous
{
    using System;

    /// <summary>
    /// Port for sending requests and receiving repplies.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequestPort<in TRequest, TReply>
    {
        /// <summary>
        /// Send an asynchronous request, and let the reply be delivered to the fiber when ready
        /// </summary>
        /// <param name="request"></param>
        /// <param name="fiber"></param>
        /// <param name="onReply"></param>
        /// <returns></returns>
        IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply);

        /// <summary>
        /// Send an asynchronous request and get a reply object for handling the response in the same code block.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        IReply<TReply> SendRequest(TRequest request);
    }

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

    /// <summary>
    /// Future type for receiving a response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReply<T>
    {
        /// <summary>
        /// Call to wait for a reply to be delivereed.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IResult<T> Receive(TimeSpan timeout);
    }

    /// <summary>
    /// Reponse to a request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResult<T>
    {
        /// <summary>
        /// Did we successfully receive a reply
        /// </summary>
        bool IsValid { get; }
        /// <summary>
        /// The rpely value
        /// </summary>
        T Value { get; }
    }

    /// <summary>
    /// Port for setting up request handling fibers
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequestHandlerPort<out TRequest, in TReply>
    {
        /// <summary>
        /// Set the fiber and handler for responding to requests.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="onRequest"></param>
        /// <returns></returns>
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }

    

    /// <summary>
    /// Interface for requests where a handler can send a reply
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequest<out TRequest, in TReply>
    {
        /// <summary>
        /// The request 
        /// </summary>
        TRequest Request { get; }

        /// <summary>
        /// Reply to the request 
        /// </summary>
        /// <param name="reply"></param>
        void Reply(TReply reply);
    }
}