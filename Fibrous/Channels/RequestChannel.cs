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

        public Task<TReply> SendRequest(TRequest request)
        {
            var channelRequest = new ChannelRequest(request);
            _requestChannel.Publish(channelRequest);
            return channelRequest.Resp.Task;
        }

        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            var channelRequest = new AsyncChannelRequest(fiber, request, onReply);
            _requestChannel.Publish(channelRequest);
            return new Unsubscriber(channelRequest, fiber);
        }

        private sealed class ChannelRequest : IRequest<TRequest, TReply>, IDisposable
        {
            private readonly object _lock = new object();
            private bool _disposed;
            private bool _replied;

            public ChannelRequest(TRequest req)
            {
                Request = req;
            }

            //don't use a queue
            public TaskCompletionSource<TReply> Resp { get; } = new TaskCompletionSource<TReply>();

            public void Dispose()
            {
                lock (_lock)
                {
                    _replied = true;
                    _disposed = true;
                    Monitor.PulseAll(_lock);
                }
            }

            public TRequest Request { get; }

            public void Reply(TReply response)
            {
                lock (_lock)
                {
                    if (_replied || _disposed) return;
                    Resp.SetResult(response);
                    Monitor.PulseAll(_lock);
                }
            }
        }

        private class AsyncChannelRequest : IRequest<TRequest, TReply>, IDisposable
        {
            private readonly IChannel<TReply> _resp = new Channel<TReply>();
            private readonly IDisposable _sub;

            public AsyncChannelRequest(IFiber fiber, TRequest request, Action<TReply> replier)
            {
                Request = request;
                _sub = _resp.Subscribe(fiber, replier);
            }

            public void Dispose()
            {
                _sub?.Dispose();
            }

            public TRequest Request { get; }

            public void Reply(TReply response)
            {
                _resp.Publish(response);
            }
        }


    }
}