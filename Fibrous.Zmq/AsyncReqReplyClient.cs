namespace Fibrous.Zmq
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using ZeroMQ;
    using ZeroMQ.Sockets;

    public class AsyncReqReplyClient<TRequest, TReply> : IAsyncRequestPort<TRequest, TReply>, IDisposable
    {
        //I can apparently get rid of this and use XREQ/XREP sockets
        private readonly byte[] _id = GetId();
        private readonly IAsyncRequestReplyChannel<TRequest, TReply> _internalChannel =
            new AsyncRequestReplyChannel<TRequest, TReply>();
        private readonly IFiber _fiber;
        private volatile bool _running = true;
        private readonly IZmqContext _replyContext;
        private readonly ISubscribeSocket _replySocket;
        private readonly Func<byte[], TReply> _replyUnmarshaller;
        private readonly ISendSocket _requestSocket;
        private readonly Func<TRequest, byte[]> _requestMarshaller;
        private readonly Dictionary<Guid, IRequest<TRequest, TReply>> _requests =
            new Dictionary<Guid, IRequest<TRequest, TReply>>();
        private readonly Task _task;

        private static byte[] GetId()
        {
            return Guid.NewGuid().ToByteArray();
        }

        public AsyncReqReplyClient(string requestAddress,
                                   string replyAddress,
                                   Func<TRequest, byte[]> requestMarshaller,
                                   Func<byte[], TReply> replyUnmarshaller)
            : this(requestAddress, replyAddress, requestMarshaller, replyUnmarshaller, new PoolFiber())
        {
        }

        public AsyncReqReplyClient(string requestAddress,
                                   string replyAddress,
                                   Func<TRequest, byte[]> requestMarshaller,
                                   Func<byte[], TReply> replyUnmarshaller,
                                   IFiber fiber)
        {
            _requestMarshaller = requestMarshaller;
            _replyUnmarshaller = replyUnmarshaller;
            _fiber = fiber;
            _internalChannel.SetRequestHandler(_fiber, OnRequest);
            //set up sockets and subscribe to pub socket
            _replyContext = ZmqContext.Create(1);
            _replySocket = _replyContext.CreateSubscribeSocket();
            _replySocket.Connect(replyAddress);
            _replySocket.Subscribe(_id);
            //  _requestContext = ZmqContext.Create(1);
            _requestSocket = _replyContext.CreatePushSocket();
            _requestSocket.Connect(requestAddress);
            _fiber.Start();
            _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            while (_running)
            {
                //check for time/cutoffs to trigger events...
                byte[] id = _replySocket.Receive(TimeSpan.FromMilliseconds(100));
                if (id == null || id.Length == 0 || !_running)
                {
                    continue;
                }
                byte[] reqId = _replySocket.Receive(TimeSpan.FromSeconds(1));
                if (reqId == null || reqId.Length != 16)
                {
                    //ERROR
                    throw new Exception("Got id but no msg id");
                }
                var guid = new Guid(reqId);
                if (!_requests.ContainsKey(guid))
                {
                    throw new Exception("We don't have a msg SenderId for this reply");
                }
                byte[] data = _replySocket.Receive(TimeSpan.FromSeconds(1));
                if (data == null)
                {
                    //ERROR
                    throw new Exception("Got ids but no data");
                }
                TReply reply = _replyUnmarshaller(data);
                _fiber.Enqueue(() => Send(guid, reply));
            }
            InternalDispose();
        }

        private void Send(Guid guid, TReply reply)
        {
            IRequest<TRequest, TReply> request = _requests[guid];
            request.Publish(reply);
        }

        private void InternalDispose()
        {
            _replySocket.Dispose();
            _requestSocket.Dispose();
            _replyContext.Dispose();
            //_requestContext.Dispose();
            _fiber.Dispose();
        }

        private void OnRequest(IRequest<TRequest, TReply> obj)
        {
            //serialize and compress and send...
            byte[] msgId = GetId();
            _requests[new Guid(msgId)] = obj;
            _requestSocket.SendPart(_id);
            _requestSocket.SendPart(msgId);
            byte[] requestData = _requestMarshaller(obj.Request);
            _requestSocket.Send(requestData);
        }

        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            return _internalChannel.SendRequest(request, fiber, onReply);
        }

        public void Dispose()
        {
            _running = false;
        }
    }
}