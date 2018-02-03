namespace Fibrous
{
    using System;

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
}