using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IRequestChannel<T, T1> : IRequestPort<T, T1>, IRequestHandlerPort<T, T1>
    {
    }

    /// <summary>
    ///     Port for sending requests and receiving repplies.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequestPort<in TRequest, TReply>
    {
        /// <summary>
        ///     Send an asynchronous request, and let the reply be delivered to the fiber when ready
        /// </summary>
        /// <param name="request"></param>
        /// <param name="fiber"></param>
        /// <param name="onReply"></param>
        /// <returns></returns>
        IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply);

        /// <summary>
        ///     Send an asynchronous request and get a reply object for handling the response in the same code block.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TReply> SendRequest(TRequest request);
    }

    /// <summary>
    ///     Interface for requests where a handler can send a reply
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequest<out TRequest, in TReply>
    {
        /// <summary>
        ///     The request
        /// </summary>
        TRequest Request { get; }

        /// <summary>
        ///     Reply to the request
        /// </summary>
        /// <param name="reply"></param>
        void Reply(TReply reply);
    }

    /// <summary>
    ///     Port for setting up request handling fibers
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequestHandlerPort<out TRequest, in TReply>
    {
        /// <summary>
        ///     Set the fiber and handler for responding to requests.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="onRequest"></param>
        /// <returns></returns>
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);
    }


    //public static class RequestPortExtensions
    //{
    //    /// <summary>
    //    ///     Send a request with infinite timeout
    //    /// </summary>
    //    /// <typeparam name="TRequest"></typeparam>
    //    /// <typeparam name="TReply"></typeparam>
    //    /// <param name="port"></param>
    //    /// <param name="request"></param>
    //    /// <returns></returns>
    //    public static async Task<TReply> SendRequest<TRequest, TReply>(this IRequestPort<TRequest, TReply> port, TRequest request)
    //    {
    //        return await port.SendRequest(request);
    //    }
    //}
}