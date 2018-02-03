namespace Fibrous
{
    using System;

    /// <summary>
    /// Port for sending requests and receiving repplies.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequestPort<in TRequest, out TReply>
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
}