using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fibrous.Channels;
using Fibrous.Fibers;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public class AsyncReqReplyClient<TRequest, TReply> : IAsyncRequestPort<TRequest, TReply>, IDisposable
    {
        //I can apparently get rid of this and use XREQ/XREP sockets
        private readonly byte[] _id = GetId();

        private readonly IAsyncRequestReplyChannel<TRequest, TReply> _internalChannel
            = new AsyncRequestReplyChannel<TRequest, TReply>();

        private readonly IFiber _fiber;
        private volatile bool _running = true;

        private readonly ZmqContext _replyContext;

        private readonly ZmqSocket _replySocket;
        private readonly Func<byte[], int, TReply> _replyUnmarshaller;
        private readonly ZmqSocket _requestSocket;
        private readonly Func<TRequest, byte[]> _requestMarshaller;

        private readonly Dictionary<Guid, IRequest<TRequest, TReply>> _requests
            = new Dictionary<Guid, IRequest<TRequest, TReply>>();

        private readonly Task _task;

        private static byte[] GetId()
        {
            return Guid.NewGuid().ToByteArray();
        }

        public AsyncReqReplyClient(string requestAddress, string replyAddress,
                                   Func<TRequest, byte[]> requestMarshaller,
                                   Func<byte[], int, TReply> replyUnmarshaller, int bufferSize)
            : this(requestAddress, replyAddress, requestMarshaller, replyUnmarshaller,bufferSize, new PoolFiber())
        {
        }

        public AsyncReqReplyClient(string requestAddress, string replyAddress,
                                   Func<TRequest, byte[]> requestMarshaller,
                                   Func<byte[], int, TReply> replyUnmarshaller, int bufferSize, IFiber fiber)
        {
            _requestMarshaller = requestMarshaller;
            _replyUnmarshaller = replyUnmarshaller;
            _fiber = fiber;
            data = new byte[bufferSize];

            _internalChannel.SetRequestHandler(_fiber, OnRequest);

            //set up sockets and subscribe to pub socket
            _replyContext = ZmqContext.Create();
            _replySocket = _replyContext.CreateSocket(SocketType.SUB);
            _replySocket.Connect(replyAddress);
            _replySocket.Subscribe(_id);

            _requestSocket = _replyContext.CreateSocket(SocketType.PUSH);
            _requestSocket.Connect(requestAddress);


            _fiber.Start();
            _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }
        byte[] id = new byte[16];
        byte[] reqId = new byte[16];
        private byte[] data;
        private void Run()
        {
            while (_running)
            {
                //check for time/cutoffs to trigger events...

             //   byte[] id = new byte[16];
                int idCount = _replySocket.Receive(id, TimeSpan.FromMilliseconds(100));
                if (idCount == 0)
                    continue;

                int reqIdCount =  _replySocket.Receive(reqId,TimeSpan.FromSeconds(3));
                if (reqIdCount != 16)
                {
                    //ERROR
                    throw new Exception("Got id but no msg id");
                }
                var guid = new Guid(reqId);
                if (!_requests.ContainsKey(guid))
                {
                    throw new Exception("We don't have a msg SenderId for this reply");
                }
               int dataLength = _replySocket.Receive(data, TimeSpan.FromSeconds(3));
               if (dataLength == 0)
                {
                    //ERROR
                    throw new Exception("Got ids but no data");
                }

                TReply reply = _replyUnmarshaller(data, dataLength);
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
            _requestSocket.SendMore(_id);
            _requestSocket.SendMore(msgId);
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