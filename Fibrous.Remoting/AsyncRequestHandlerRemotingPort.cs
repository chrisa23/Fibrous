namespace Fibrous.Remoting
{
    using System;
    using System.Threading.Tasks;
    using CrossroadsIO;
    using Fibrous.Channels;
    using Fibrous.Fibers;

    public class AsyncRequestHandlerRemotingPort<TRequest, TReply> : IRequestHandlerPort<TRequest, TReply>, IDisposable
    {
        private readonly IAsyncRequestChannel<TRequest, TReply> _internalChannel = new AsyncRequestChannel<TRequest, TReply>();
        //split InSocket
        private readonly Context _context;
        private readonly IFiber _stub = new StubFiber();
        private readonly Func<TReply, byte[]> _replyMarshaller;
        //split OutSocket
        private readonly Socket _replySocket;
        private readonly Socket _requestSocket;
        private readonly Func<byte[], TRequest> _requestUnmarshaller;
        private volatile bool _running = true;
        private readonly Task _task;
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

        public AsyncRequestHandlerRemotingPort(Context context,
                                               string address,
                                               int basePort,
                                               Func<byte[], TRequest> requestUnmarshaller,
                                               Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _replyMarshaller = replyMarshaller;
            _context = context;
            _requestSocket = _context.CreateSocket(SocketType.PULL);
            _requestSocket.Bind(address + ":" + basePort);
            _replySocket = _context.CreateSocket(SocketType.PUB);
            _replySocket.Bind(address + ":" + (basePort + 1));
            _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            _running = false;
        }

        private void Run()
        {
            while (_running)
            {
                Message message = _requestSocket.ReceiveMessage(_timeout);
                if (message.IsEmpty)
                    continue;
                byte[] id = message[0].Buffer;
                byte[] rid = message[1].Buffer;
                ProcessRequest(id, rid, message[2].Buffer);
            }
            InternalDispose();
        }

        private void ProcessRequest(byte[] id, byte[] msgId, byte[] msgBuffer)
        {
            TRequest req = _requestUnmarshaller(msgBuffer);
            _internalChannel.SendRequest(req, _stub, reply => SendReply(id, msgId, reply));
        }

        private void SendReply(byte[] id, byte[] msgId, TReply reply)
        {
            byte[] data = _replyMarshaller(reply);
            _replySocket.SendMore(id);
            _replySocket.SendMore(msgId);
            _replySocket.Send(data);
        }

        private void InternalDispose()
        {
            _requestSocket.Dispose();
            _replySocket.Dispose();
        }

        public IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest)
        {
            return _internalChannel.SetRequestHandler(fiber, onRequest);
        }
    }
}