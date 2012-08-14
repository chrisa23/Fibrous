namespace Fibrous.Channels
{
    using System;

    public sealed class AsyncRequestReplyChannel<TRequest, TReply> : IAsyncRequestReplyChannel<TRequest, TReply>
    {
        private readonly IChannel<IRequest<TRequest, TReply>> _requestChannel =
            new Channel<IRequest<TRequest, TReply>>();

        public IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest)
        {
            return _requestChannel.Subscribe(fiber, onRequest);
        }

        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            var channelRequest = new AsyncChannelRequest(fiber, request, onReply);
            bool sent = _requestChannel.Publish(channelRequest);
            if (!sent)
            {
                throw new ArgumentException("No one is listening on AsyncRequestReplyChannel");
            }
            return channelRequest;
        }

        private class AsyncChannelRequest : IRequest<TRequest, TReply>, IDisposable
        {
            private readonly TRequest _request;
            private readonly IChannel<TReply> _resp = new Channel<TReply>();
            private readonly IDisposable _sub;

            public AsyncChannelRequest(IFiber fiber, TRequest request, Action<TReply> replier)
            {
                _request = request;
                _sub = _resp.Subscribe(fiber, replier);
            }

            public TRequest Request { get { return _request; } }

            public bool PublishReply(TReply response)
            {
                return _resp.Publish(response);
            }

            public void Dispose()
            {
                if (_sub != null)
                {
                    _sub.Dispose();
                }
            }
        }
    }
}