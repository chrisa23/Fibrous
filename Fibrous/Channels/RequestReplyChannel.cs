using System;
using System.Collections.Generic;
using System.Threading;

namespace Fibrous.Channels
{
    public sealed class RequestReplyChannel<TRequest, TReply> : IRequestReplyChannel<TRequest, TReply>
    {
        private readonly IChannel<IRequest<TRequest, TReply>> _requestChannel =
            new Channel<IRequest<TRequest, TReply>>();

        public IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest)
        {
            return _requestChannel.Subscribe(fiber, onRequest);
        }

        public TReply SendRequest(TRequest request, TimeSpan timeout)
        {
            using (var channelRequest = new ChannelRequest(request))
            {
                _requestChannel.Publish(channelRequest);
                TReply reply;
                if (!channelRequest.Receive(timeout, out reply))
                {
                    throw new TimeoutException("Timeout waiting for reply");
                }
                return reply;
            }
        }

        private sealed class ChannelRequest : IRequest<TRequest, TReply>, IDisposable
        {
            private readonly object _lock = new object();
            private readonly TRequest _req;
            private readonly Queue<TReply> _resp = new Queue<TReply>();
            private bool _disposed;

            public ChannelRequest(TRequest req)
            {
                _req = req;
            }

            public TRequest Request
            {
                get { return _req; }
            }

            public bool Publish(TReply response)
            {
                lock (_lock)
                {
                    if (_disposed)
                    {
                        return false;
                    }
                    _resp.Enqueue(response);
                    Monitor.PulseAll(_lock);
                    return true;
                }
            }

            public bool Receive(TimeSpan timeout, out TReply result)
            {
                lock (_lock)
                {
                    if (_resp.Count > 0)
                    {
                        result = _resp.Dequeue();
                        return true;
                    }
                    if (_disposed)
                    {
                        result = default(TReply);
                        return false;
                    }
                    Monitor.Wait(_lock, timeout);
                    if (_resp.Count > 0)
                    {
                        result = _resp.Dequeue();
                        return true;
                    }
                }
                result = default(TReply);
                return false;
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    _disposed = true;
                    Monitor.PulseAll(_lock);
                }
            }
        }
    }
}