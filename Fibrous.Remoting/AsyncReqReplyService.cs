namespace Fibrous.Zmq
{
    using System;
    using System.Threading.Tasks;
    using CrossroadsIO;

    public class AsyncReqReplyService<TRequest, TReply> : IDisposable
    {
        private readonly Func<byte[], int, TRequest> _requestUnmarshaller;
        private readonly Func<TRequest, TReply> _businessLogic;
        private readonly Func<TReply, byte[]> _replyMarshaller;
        //split InSocket
        private readonly Context _context;
        private readonly Socket _requestSocket;
        //split OutSocket
        private readonly Socket _replySocket;
        private volatile bool _running = true;

        public AsyncReqReplyService(string requestAddress,
                                    string replyAddress,
                                    Func<byte[], int, TRequest> requestUnmarshaller,
                                    Func<TRequest, TReply> businessLogic,
                                    Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _businessLogic = businessLogic;
            _replyMarshaller = replyMarshaller;
            _context = Context.Create();
            _requestSocket = _context.CreateSocket(SocketType.PULL);
            _requestSocket.Bind(requestAddress);
            _replySocket = _context.CreateSocket(SocketType.PUB);
            _replySocket.Bind(replyAddress);
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
                if (idCount == 0 || !_running) //?? not sure on this
                {
                    continue;
                }
                int reqIdCount = _requestSocket.Receive(reqId, TimeSpan.FromSeconds(1));
                if (reqIdCount != 16)
                {
                    throw new Exception("We don't have a msg SenderId for this request");
                }
                int dataCount = _requestSocket.Receive(data, TimeSpan.FromSeconds(1));
                if (dataCount == 0)
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
            TReply reply = _businessLogic(req);
            SendReply(id, msgId, reply);
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
            _context.Dispose();
        }

        public void Dispose()
        {
            _running = false;
        }
    }
}