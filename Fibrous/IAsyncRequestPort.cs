using System;

namespace Fibrous
{
    /// <summary>
    /// Sends a request with a callback and fiber for replies
    /// Can receive multiple replies.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IAsyncRequestPort<in TRequest, out TReply>
    {
        IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply);
    }
}