namespace Fibrous.Remoting
{
    using System;
    using System.Threading.Tasks;
    using NetMQ;
    using NetMQ.zmq;

    //takes 2 ports
    public sealed class AsyncRequestHandlerSocket<TRequest, TReply> : IRequestHandlerPort<TRequest, TReply>, IDisposable
    {
        private readonly IRequestChannel<TRequest, TReply> _internalChannel = new RequestChannel<TRequest, TReply>();
        //split InSocket
        private readonly NetMQContext _context;
        private readonly IFiber _stub = StubFiber.StartNew();
        private readonly Func<TReply, byte[]> _replyMarshaller;
        //split OutSocket
        private readonly NetMQSocket _replySocket;
        private readonly NetMQSocket _requestSocket;
        private readonly Func<byte[], TRequest> _requestUnmarshaller;
        private volatile bool _running = true;

        public AsyncRequestHandlerSocket(NetMQContext context,
                                         string address,
                                         Func<byte[], TRequest> requestUnmarshaller,
                                         Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _replyMarshaller = replyMarshaller;
            _context = context;
            string s = address.Split(':')[2];
            int basePort = int.Parse(s);
            _requestSocket = _context.CreateSocket(ZmqSocketType.Pull);
            _requestSocket.Bind(address);
            _replySocket = _context.CreateSocket(ZmqSocketType.Pub);
            _replySocket.Bind(address.Substring(0, address.LastIndexOf(":")) + ":" + (basePort + 1));
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            _requestSocket.Close();
        }

        private void Run()
        {
            while (_running)
            {
                try
                {
                    NetMQMessage message = _requestSocket.ReceiveMessage(false);
                    if (message.IsEmpty)
                        continue;
                    byte[] id = message[0].Buffer;
                    byte[] rid = message[1].Buffer;
                    ProcessRequest(id, rid, message[2].Buffer);
                }
                catch (Exception e)
                {
                    _running = false;
                }
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