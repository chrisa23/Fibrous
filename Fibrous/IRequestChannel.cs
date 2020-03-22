using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IRequestChannel<TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
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

        CancellationToken CancellationToken { get; }
    }
}