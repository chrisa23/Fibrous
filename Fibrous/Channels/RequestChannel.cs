using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous
{
    public sealed class RequestChannel<TRequest, TReply> : IRequestChannel<TRequest, TReply>
    {
        private readonly IChannel<IRequest<TRequest, TReply>> _requestChannel =
            new Channel<IRequest<TRequest, TReply>>();

        public IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest)
        {
            return _requestChannel.Subscribe(fiber, onRequest);
        }

        public IDisposable SetRequestHandler(IAsyncFiber fiber, Func<IRequest<TRequest, TReply>, Task> onRequest)
        {
            return _requestChannel.Subscribe(fiber, onRequest);
        }

        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            var channelRequest = new AsyncChannelRequest(fiber, request, onReply);
            _requestChannel.Publish(channelRequest);
            return new Unsubscriber(channelRequest, fiber);
        }

        public IDisposable SendRequest(TRequest request, IAsyncFiber fiber, Func<TReply, Task> onReply)
        {
            var channelRequest = new AsyncChannelRequest(fiber, request, onReply);
            _requestChannel.Publish(channelRequest);
            return new Unsubscriber(channelRequest, fiber);
        }

        public Task<TReply> SendRequest(TRequest request)
        {
            var channelRequest = new ChannelRequest(request);
            _requestChannel.Publish(channelRequest);
            return channelRequest.Resp.Task;
        }

        /// <summary>
        ///     Async ReqReply with timeout
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Result<TReply>> SendRequest(TRequest request, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            using var channelRequest = new ChannelRequest(request, cts);
            _requestChannel.Publish(channelRequest);
            try
            {
                var reply = await channelRequest.Resp.Task;
                return Result<TReply>.Ok(reply);
            }
            catch (TaskCanceledException)
            {
                return Result<TReply>.Failed;
            }
        }
        
        public sealed class ChannelRequest : IRequest<TRequest, TReply>, IDisposable
        {
            private readonly SingleShotGuard _guard = new SingleShotGuard();
            private readonly CancellationTokenSource _cancel;
           
            public ChannelRequest(TRequest req, CancellationTokenSource cts = null)
            {
                Request = req;
                _cancel = cts ?? new CancellationTokenSource();
                if(cts != null)
                    _cancel.Token.Register(Callback);
            }

            private void Callback()
            {
                Resp.TrySetCanceled();
            }

            public TaskCompletionSource<TReply> Resp { get; } = new TaskCompletionSource<TReply>();
            public CancellationToken CancellationToken => _cancel.Token;

            public TRequest Request { get; }

            public void Reply(TReply response)
            {
                if (_guard.Check)
                {
                    Resp.SetResult(response);
                }
            }

            public void Dispose()
            {
                if (_guard.Check)
                {
                    _cancel.Cancel();
                    
                }
            }
        }

        public class AsyncChannelRequest : IRequest<TRequest, TReply>, IDisposable
        {
            private readonly SingleShotGuard _guard = new SingleShotGuard();
            private readonly IChannel<TReply> _resp = new Channel<TReply>();
            private readonly IDisposable _sub;
            private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
            public AsyncChannelRequest(IFiber fiber, TRequest request, Action<TReply> replier)
            {
                Request = request;
                _sub = _resp.Subscribe(fiber, replier);
            }

            public AsyncChannelRequest(IAsyncFiber fiber, TRequest request, Func<TReply, Task> replier)
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

        public void Dispose()
        {
            _requestChannel.Dispose();
        }
    }
}