namespace Fibrous.Remoting
{
    using System;
    using System.Threading.Tasks;
    using CrossroadsIO;
    using Fibrous.Fibers;

    public class AsyncRequestService<TRequest, TReply> : IDisposable
    {
        private readonly Func<byte[], int, TRequest> _requestUnmarshaller;
        private readonly IAsyncRequestPort<TRequest, TReply> _businessLogic; 
        private readonly Func<TReply, byte[]> _replyMarshaller;
        //split InSocket
        private readonly Context _context;
        private readonly Socket _requestSocket;
        //split OutSocket
        private readonly Socket _replySocket;
        private volatile bool _running = true;
        private readonly IFiber _fiber = PoolFiber.StartNew();

        public AsyncRequestService(Context context,
                                   string address,
                                   int basePort,
                                   Func<byte[], int, TRequest> requestUnmarshaller,
                                   IAsyncRequestPort<TRequest, TReply> logic,
                                   Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _businessLogic = logic;
            _replyMarshaller = replyMarshaller;
            _context = context;
            _requestSocket = _context.CreateSocket(SocketType.PULL);
            _requestSocket.Bind(address + ":" + basePort);
            _replySocket = _context.CreateSocket(SocketType.PUB);
            _replySocket.Bind(address + ":" + (basePort + 1));
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }
    
        private void Run()
        {
            while (_running)
            {

                Message message = _requestSocket.ReceiveMessage();
                if(message.IsEmpty)
                {
                    continue;
                }
                var id = message[0].Buffer;
                var rid = message[1].Buffer;
                ProcessRequest(id, rid, message[2].Buffer, message[2].BufferSize);
            }
            InternalDispose();
        }

        private void ProcessRequest(byte[] id, byte[] msgId, byte[] msgBuffer, int length)
        {
            TRequest req = _requestUnmarshaller(msgBuffer, length);
            _businessLogic.SendRequest(req, _fiber, reply => SendReply(id, msgId, reply));
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

        public void Dispose()
        {
            _running = false;
        }
    }
}