namespace Fibrous.Remoting
{
    using System;
    using System.Threading.Tasks;
    using CrossroadsIO;
    using Fibrous.Fibers;

    //this does not lend itself to parallel workers for handling requests
    public class AsyncReqReplyService<TRequest, TReply> : IDisposable
    {
        private readonly Func<byte[], int, TRequest> _requestUnmarshaller;
        private readonly IAsyncRequestPort<TRequest, TReply> _businessLogic; //Action<RequestWrapper>
        private readonly Func<TReply, byte[]> _replyMarshaller;
        //split InSocket
        private readonly Context _context;
        private readonly Socket _requestSocket;
        //split OutSocket
        private readonly Socket _replySocket;
        private volatile bool _running = true;
        private readonly IFiber _fiber = PoolFiber.StartNew();

        public AsyncReqReplyService(Context context,
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

        private readonly byte[] id = new byte[16];
        private readonly byte[] reqId = new byte[16];
        private readonly byte[] data = new byte[1024 * 1024 * 2];

        private void Run()
        {
            while (_running)
            {
                //check for time/cutoffs to trigger events...
                int idCount = _requestSocket.Receive(id, TimeSpan.FromMilliseconds(100));
                if (idCount == -1 || !_running) //?? not sure on this
                {
                    continue;
                }
                int reqIdCount = _requestSocket.Receive(reqId, TimeSpan.FromSeconds(1));
                if (reqIdCount != 16)
                {
                    throw new Exception("We don't have a msg SenderId for this request");
                }
                int dataCount = _requestSocket.Receive(data, TimeSpan.FromSeconds(1));
                if (dataCount == -1)
                {
                    throw new Exception("We don't have a msg for the request");
                }
                ProcessRequest(id, reqId, data, dataCount);
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