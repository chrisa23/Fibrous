using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous;

public interface IRequestChannel<TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
{
    /// <summary>
    ///     Sets the fiber and handler that will process incoming requests.
    /// </summary>
    /// <param name="fiber">Fiber that handles incoming requests.</param>
    /// <param name="onRequest">Handler invoked for each request.</param>
    IDisposable SetRequestHandler(IFiber fiber, Func<IRequest<TRequest, TReply>, Task> onRequest);
}

/// <summary>
///     Port for sending requests and receiving replies.
/// </summary>
public interface IRequestPort<in TRequest, TReply>
{
    /// <summary>
    ///     Sends an asynchronous request with cancellation support.
    /// </summary>
    /// <param name="request">Request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    Task<Reply<TReply>> SendRequestAsync(TRequest request, CancellationToken cancellationToken);

    /// <summary>
    ///     Sends an asynchronous request with a timeout.
    /// </summary>
    /// <param name="request">Request payload.</param>
    /// <param name="timeout">Timeout before a failed reply result is returned.</param>
    Task<Reply<TReply>> SendRequestAsync(TRequest request, TimeSpan timeout);

    /// <summary>
    ///     Sends an asynchronous request and returns the reply task.
    /// </summary>
    /// <param name="request">Request payload.</param>
    Task<TReply> SendRequestAsync(TRequest request);

    /// <summary>
    ///     Sends an asynchronous request and delivers the reply to a fiber when ready.
    /// </summary>
    /// <param name="request">Request payload.</param>
    /// <param name="fiber">Fiber that receives the reply.</param>
    /// <param name="onReply">Reply handler.</param>
    IDisposable SendRequest(TRequest request, IFiber fiber, Func<TReply, Task> onReply);

    /// <summary>
    ///     Sends an asynchronous request and delivers the reply to a fiber when ready.
    /// </summary>
    /// <param name="request">Request payload.</param>
    /// <param name="fiber">Fiber that receives the reply.</param>
    /// <param name="onReply">Reply handler.</param>
    IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply);
}

/// <summary>
///     Interface for requests where a handler can send a reply
/// </summary>
public interface IRequest<out TRequest, in TReply>
{
    /// <summary>
    ///     Gets the request payload.
    /// </summary>
    TRequest Request { get; }

    CancellationToken CancellationToken { get; }

    /// <summary>
    ///     Replies to the request.
    /// </summary>
    /// <param name="reply">Reply payload.</param>
    void Reply(TReply reply);
}

public readonly struct Reply<T>
{
    public readonly T Value;
    public readonly bool Succeeded;

    private Reply(T value)
    {
        Succeeded = true;
        Value     = value;
    }

    public static Reply<T> Ok(T value) => new(value);
    public static readonly Reply<T> Failed = default;
}
