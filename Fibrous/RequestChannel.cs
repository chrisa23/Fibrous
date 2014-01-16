namespace Fibrous
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class RequestChannel<TRequest, TReply> : IRequestChannel<TRequest, TReply>
    {
        private readonly IChannel<IRequest<TRequest, TReply>> _requestChannel =
            new Channel<IRequest<TRequest, TReply>>();

        public IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest)
        {
            return _requestChannel.Subscribe(fiber, onRequest);
        }

        private sealed class ChannelRequest : IRequest<TRequest, TReply>, IReply<TReply>, IDisposable
        {
            private readonly object _lock = new object();
            private readonly TRequest _req;
            //don't use a queue
            private readonly Queue<TReply> _resp = new Queue<TReply>();
            private bool _disposed;
            private bool _replied;

            public ChannelRequest(TRequest req)
            {
                _req = req;
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    _replied = true;
                    _disposed = true;
                    Monitor.PulseAll(_lock);
                }
            }

            public TRequest Request { get { return _req; } }

            public void Reply(TReply response)
            {
                lock (_lock)
                {
                    if (_replied || _disposed) return;
                    _resp.Enqueue(response);
                    Monitor.PulseAll(_lock);
                }
            }

            public Result<TReply> Receive(TimeSpan timeout)
            {
                lock (_lock)
                {
                    if (_replied)
                        return new Result<TReply>();
                    if (_resp.Count > 0)
                    {
                        _replied = true;
                        return new Result<TReply>(_resp.Dequeue());
                    }
                    if (_disposed)
                    {
                        _replied = true;
                        return new Result<TReply>();
                    }
                    Monitor.Wait(_lock, timeout);
                    if (_resp.Count > 0)
                    {
                        _replied = true;
                        return new Result<TReply>(_resp.Dequeue());
                    }
                }
                return new Result<TReply>();
            }
        }

        public IReply<TReply> SendRequest(TRequest request)
        {
            var channelRequest = new ChannelRequest(request);
            _requestChannel.Publish(channelRequest);
            return channelRequest;
        }

        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            var channelRequest = new AsyncChannelRequest(fiber, request, onReply);
            _requestChannel.Publish(channelRequest);
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

            public void Dispose()
            {
                if (_sub != null)
                    _sub.Dispose();
            }

            public TRequest Request { get { return _request; } }

            public void Reply(TReply response)
            {
                _resp.Publish(response);
            }
        }
    }
}