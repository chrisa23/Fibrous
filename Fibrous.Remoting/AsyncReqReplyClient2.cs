namespace Fibrous.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using CrossroadsIO;
    using Fibrous.Channels;
    using Fibrous.Fibers;

    public class AsyncReqReplyClient2<TRequest, TReply> : IAsyncRequestPort<TRequest, TReply>, IDisposable
    {
        private readonly IAsyncRequestReplyChannel<TRequest, TReply> _internalChannel =
            new AsyncRequestReplyChannel<TRequest, TReply>();
        private volatile bool _running = true;
        private readonly Context _context;
        private readonly Socket _socket;
        private readonly Func<byte[], int, TReply> _replyUnmarshaller;
        private readonly Func<TRequest, byte[]> _requestMarshaller;
        
        private readonly Dictionary<Guid, IRequest<TRequest, TReply>> _requests = new Dictionary<Guid, IRequest<TRequest, TReply>>();
        private readonly IFiber _fiber;
        private readonly Task _task;

        private readonly byte[] _buffer = new byte[1024 * 1024 * 2];
        private readonly byte[] _requestBuffer = new byte[1024 * 1024 * 2];

        public AsyncReqReplyClient2(string address,
                                    Func<TRequest, byte[]> requestMarshaller,
                                    Func<byte[], int, TReply> replyUnmarshaller)
            : this(address, requestMarshaller, replyUnmarshaller, new PoolFiber())
        {
        }

        public AsyncReqReplyClient2(string address,
                                    Func<TRequest, byte[]> requestMarshaller,
                                    Func<byte[], int, TReply> replyUnmarshaller,
                                    IFiber fiber)
        {
            _requestMarshaller = requestMarshaller;
            _replyUnmarshaller = replyUnmarshaller;
            _fiber = fiber;
            _fiber.Start();//??
            _internalChannel.SetRequestHandler(_fiber, OnRequest);
            //set up sockets and subscribe to pub socket
            _context = Context.Create();
            _socket = _context.CreateSocket(SocketType.XREQ);
            _socket.Connect(address);

            _fiber.Enqueue(Run);
        }

        private static byte[] GetId()
        {
            return Guid.NewGuid().ToByteArray();
        }

        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(10);

        private void Run()
        {
            if (_running)
            {
                int length = 0;
                if ((length = _socket.Receive(_buffer, _timeout)) != -1)
                {
                    ProcessReply(length);    
                }
                
                _fiber.Enqueue(Run);
            }
            if(!_running)
                InternalDispose();
        }

        private void ProcessReply(int length)
        {
            //buffer has Id right now...
            //length should be 16
            Debug.Assert(length == 16);
            Debug.Assert(_socket.ReceiveMore);
            byte[] guidBytes = new byte[length];
            Buffer.BlockCopy(_buffer,0,guidBytes,0,16);
            
            int bodyLength = _socket.Receive(_buffer);
            
            TReply reply = _replyUnmarshaller(_buffer,length);
            var guid = new Guid(guidBytes);
            Send(guid, reply);
        }

        private void Send(Guid guid, TReply reply)
        {
            IRequest<TRequest, TReply> request = _requests[guid];
            request.PublishReply(reply);
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

            _socket.SendMore(msgId);
            _socket.Send(requestData);
        }

        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            return _internalChannel.SendRequest(request, fiber, onReply);
        }

        public void Dispose()
        {
            _running = false;
            _requests.Clear();
        }
    }
}