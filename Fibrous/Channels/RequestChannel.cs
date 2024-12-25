using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous;

public sealed class RequestChannel<TRequest, TReply> : IRequestChannel<TRequest, TReply>
{
    private readonly IChannel<IRequest<TRequest, TReply>> _requestChannel =
        new Channel<IRequest<TRequest, TReply>>();

    public IDisposable SetRequestHandler(IFiber fiber, Func<IRequest<TRequest, TReply>, Task> onRequest) =>
        _requestChannel.Subscribe(fiber, onRequest);

    public IDisposable SendRequest(TRequest request, IFiber fiber, Func<TReply, Task> onReply)
    {
        AsyncChannelRequest channelRequest = new(fiber, request, onReply);
        _requestChannel.Publish(channelRequest);
        return new Unsubscriber(channelRequest, fiber);
    }

    public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply) => SendRequest(request, fiber, onReply.ToAsync());

    public Task<TReply> SendRequestAsync(TRequest request)
    {
        ChannelRequest channelRequest = new(request);
        _requestChannel.Publish(channelRequest);
        return channelRequest.Resp.Task;
    }

    /// <summary>
    ///     Async ReqReply with timeout
    /// </summary>
    /// <param name="request"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<Reply<TReply>> SendRequestAsync(TRequest request, TimeSpan timeout)
    {
        using CancellationTokenSource cts = new(timeout);
        using ChannelRequest channelRequest = new(request, cts);
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

    public void Dispose() => _requestChannel.Dispose();

    public sealed class ChannelRequest : IRequest<TRequest, TReply>, IDisposable
    {
        private readonly CancellationTokenSource _cancel;
        private readonly SingleShotGuard _guard;

        public ChannelRequest(TRequest req, CancellationTokenSource cts = null)
        {
            Request = req;
            _cancel = cts ?? new CancellationTokenSource();
            if (cts != null)
            {
                _cancel.Token.Register(Callback);
            }
        }

        public TaskCompletionSource<TReply> Resp { get; } = new();

        public void Dispose()
        {
            if (_guard.Check)
            {
                _cancel.Cancel();
            }
        }

        public CancellationToken CancellationToken => _cancel.Token;

        public TRequest Request { get; }

        public void Reply(TReply response)
        {
            if (_guard.Check)
            {
                Resp.SetResult(response);
            }
        }

        private void Callback() => Resp.TrySetCanceled();
    }

    public class AsyncChannelRequest : IRequest<TRequest, TReply>, IDisposable
    {
        private readonly CancellationTokenSource _cancel = new();
        private readonly SingleShotGuard _guard;
        private readonly IChannel<TReply> _resp = new Channel<TReply>();
        private readonly IDisposable _sub;

        public AsyncChannelRequest(IFiber fiber, TRequest request, Func<TReply, Task> replier)
        {
            Request = request;
            _sub = _resp.Subscribe(fiber, replier);
        }

        public void Dispose()
        {
            if (_guard.Check)
            {
                _cancel.Cancel();
                _sub?.Dispose();
            }
        }

        public TRequest Request { get; }

        public void Reply(TReply response)
        {
            if (_guard.Check)
            {
                _resp.Publish(response);
            }
        }

        public CancellationToken CancellationToken => _cancel.Token;
    }
}
