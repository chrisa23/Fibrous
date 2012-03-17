using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fibrous.Channels;
using Fibrous.Fibers;
using ZeroMQ;


namespace Fibrous.Zmq
{
    public class AsyncReqReplyClient2<TRequest, TReply> : IAsyncRequestPort<TRequest, TReply>, IDisposable
    {
        private readonly IAsyncRequestReplyChannel<TRequest, TReply> _internalChannel
            = new AsyncRequestReplyChannel<TRequest, TReply>();

        private volatile bool _running = true;

        private readonly ZmqContext _context;
        private readonly ZmqSocket _socket;

        private readonly Func<byte[], TReply> _replyUnmarshaller;
        private readonly Func<TRequest, byte[]> _requestMarshaller;

        //TODO flushing of requests...
        private readonly Dictionary<Guid, IRequest<TRequest, TReply>> _requests
            = new Dictionary<Guid, IRequest<TRequest, TReply>>();

        private readonly IFiber _fiber;

        private readonly Task _task;

        private readonly ZMessage _request = new ZMessage();
        private readonly ZMessage _reply = new ZMessage();

        public AsyncReqReplyClient2(string address,
                                    Func<TRequest, byte[]> requestMarshaller,
                                    Func<byte[], TReply> replyUnmarshaller)
            : this(address, requestMarshaller, replyUnmarshaller, new PoolFiber())
        {
        }

        public AsyncReqReplyClient2(string address,
                                    Func<TRequest, byte[]> requestMarshaller,
                                    Func<byte[], TReply> replyUnmarshaller, IFiber fiber)
        {
            _requestMarshaller = requestMarshaller;
            _replyUnmarshaller = replyUnmarshaller;

            _fiber = fiber;
            _fiber.Start();
            _internalChannel.SetRequestHandler(_fiber, OnRequest);

            //set up sockets and subscribe to pub socket
            _context = ZmqContext.Create(1);
            _socket = _context.CreateSocket(SocketType.DEALER);
            _socket.Connect(address);

            _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private static byte[] GetId()
        {
            return Guid.NewGuid().ToByteArray();
        }

        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

        private void Run()
        {
            while (_running)
            {
                if (!_reply.Recv(_socket, _timeout))
                    continue;

                ProcessReply();
            }
            InternalDispose();
        }

        private void ProcessReply()
        {
            TReply reply = _replyUnmarshaller(_reply.Body);
            var guid = new Guid(_reply.MsgParts[0]);
            _fiber.Enqueue(() => Send(guid, reply));
        }

        private void Send(Guid guid, TReply reply)
        {
            IRequest<TRequest, TReply> request = _requests[guid];
            request.Publish(reply);
        }

        private void InternalDispose()
        {
            _fiber.Dispose();
            _socket.Dispose();
            _context.Dispose();
        }

        private void OnRequest(IRequest<TRequest, TReply> obj)
        {
            //serialize and compress and send...
            byte[] msgId = GetId();
            _requests[new Guid(msgId)] = obj;
            byte[] requestData = _requestMarshaller(obj.Request);
            _request.MsgParts.Clear();
            _request.Append(msgId);
            _request.Append(requestData);
            _request.Send(_socket);
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