using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IRequestChannel<TRequest, TReply> : IRequestPort<TRequest, TReply>
    {
        /// <summary>
        ///     Set the fiber and handler for responding to requests.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="onRequest"></param>
        /// <returns></returns>
        IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest);

        /// <summary>
        ///     Set the fiber and handler for responding to requests.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="onRequest"></param>
        /// <returns></returns>
        IDisposable SetRequestHandler(IAsyncFiber fiber, Func<IRequest<TRequest, TReply>, Task> onRequest);
    }

    /// <summary>
    ///     Port for sending requests and receiving replies.
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
        ///     Send an asynchronous request, and let the reply be delivered to the fiber when ready
        /// </summary>
        /// <param name="request"></param>
        /// <param name="fiber"></param>
        /// <param name="onReply"></param>
        /// <returns></returns>
        IDisposable SendRequest(TRequest request, IAsyncFiber fiber, Func<TReply, Task> onReply);

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
}