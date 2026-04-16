using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Request/reply channel for coordinating one request handler with asynchronous replies.
/// </summary>
public sealed class RequestChannel<TRequest, TReply> : IRequestChannel<TRequest, TReply>
{
    private readonly Channel<IRequest<TRequest, TReply>> _requestChannel = new();

    public IDisposable SetRequestHandler(IFiber fiber, Func<IRequest<TRequest, TReply>, Task> onRequest) =>
        _requestChannel.Subscribe(fiber, onRequest);

    public IDisposable SendRequest(TRequest request, IFiber fiber, Func<TReply, Task> onReply)
    {
        AsyncChannelRequest channelRequest = new(fiber, request, onReply);
        _requestChannel.Publish(channelRequest);
        return new Unsubscriber(channelRequest, fiber);
    }

    public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply) =>
        SendRequest(request, fiber, onReply.ToAsync());

#pragma warning disable VSTHRD003 // The returned task is completed by the request/reply channel, not by work started on the caller's context.
    public Task<TReply> SendRequestAsync(TRequest request)
    {
        ChannelRequest channelRequest = new(request);
        _requestChannel.Publish(channelRequest);
        return channelRequest.Resp.Task;
    }

    /// <summary>
    ///     Sends a request and returns either a reply or a cancellation failure.
    /// </summary>
    public async Task<Reply<TReply>> SendRequestAsync(TRequest request, CancellationToken cancellationToken)
    {
        using ChannelRequest channelRequest = new(request, cancellationToken);
        _requestChannel.Publish(channelRequest);
        try
        {
            TReply reply = await channelRequest.Resp.Task;
            return Reply<TReply>.Ok(reply);
        }
        catch (TaskCanceledException)
        {
            return Reply<TReply>.Failed;
        }
    }

    /// <summary>
    ///     Sends a request and returns either a reply or a timeout failure.
    /// </summary>
    public async Task<Reply<TReply>> SendRequestAsync(TRequest request, TimeSpan timeout)
    {
        using CancellationTokenSource cts = new(timeout);
        return await SendRequestAsync(request, cts.Token);
    }
#pragma warning restore VSTHRD003

    public void Dispose() => _requestChannel.Dispose();

    internal sealed class ChannelRequest : IRequest<TRequest, TReply>, IDisposable
    {
        private readonly CancellationTokenSource _cancel;
        private readonly CancellationTokenRegistration _registration;
        private readonly SingleShotGuard _guard;

        public ChannelRequest(TRequest req, CancellationToken cancellationToken = default)
        {
            Request = req;
            if (cancellationToken.CanBeCanceled)
            {
                _cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _registration = _cancel.Token.Register(Callback);
            }
            else
            {
                _cancel = new CancellationTokenSource();
            }
        }

        public TaskCompletionSource<TReply> Resp { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CancellationToken CancellationToken => _cancel.Token;

        public TRequest Request { get; }

        public void Dispose()
        {
            if (_guard.Check)
            {
                _registration.Dispose();
                _cancel.Cancel();
                _cancel.Dispose();
            }
        }

        public void Reply(TReply response)
        {
            if (_guard.Check)
            {
                Resp.TrySetResult(response);
            }
        }

        private void Callback() => Resp.TrySetCanceled();
    }

    private class AsyncChannelRequest : IRequest<TRequest, TReply>, IDisposable
    {
        private readonly CancellationTokenSource _cancel = new();
        private readonly Func<Task> _disposeGuardedReply;
        private readonly SingleShotGuard _guard;
        private readonly Func<TReply, Task> _replier;
        private readonly IFiber _target;
        private TReply _response;

        public AsyncChannelRequest(IFiber fiber, TRequest request, Func<TReply, Task> replier)
        {
            Request = request;
            _target = fiber;
            _replier = replier;
            _disposeGuardedReply = PublishReplyAsync;
        }

        public TRequest Request { get; }

        public CancellationToken CancellationToken => _cancel.Token;

        public void Dispose()
        {
            if (_guard.Check)
            {
                _cancel.Cancel();
                _cancel.Dispose();
            }
        }

        public void Reply(TReply response)
        {
            if (_guard.Check)
            {
                _response = response;
                _target.Enqueue(_disposeGuardedReply);
            }
        }

        private Task PublishReplyAsync() =>
            _cancel.IsCancellationRequested ? Task.CompletedTask : _replier(_response);
    }
}
