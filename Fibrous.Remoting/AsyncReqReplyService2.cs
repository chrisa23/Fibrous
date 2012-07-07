namespace Fibrous.Zmq
{
    using System;
    using System.Threading.Tasks;
    using CrossroadsIO;

    public class AsyncReqReplyService2<TRequest, TReply> : IDisposable
    {
        private readonly Func<byte[], TRequest> _requestUnmarshaller;
        private readonly Func<TRequest, TReply> _businessLogic;
        private readonly Func<TReply, byte[]> _replyMarshaller;
        private readonly Context _context;
        private readonly Socket _socket;
        private volatile bool _running = true;
        private readonly ZMessage _message = new ZMessage();
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

        public AsyncReqReplyService2(string address,
                                     Func<byte[], TRequest> requestUnmarshaller,
                                     Func<TRequest, TReply> businessLogic,
                                     Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _businessLogic = businessLogic;
            _replyMarshaller = replyMarshaller;
            _context = Context.Create();
            _socket = _context.CreateSocket(SocketType.XREP);
            _socket.ReceiveHighWatermark = 10000;
            _socket.SendHighWatermark = 10000;
            _socket.Bind(address);
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            while (_running)
            {
                //check for time/cutoffs to trigger events...
                //byte[] id = _socket.Receive(TimeSpan.FromMilliseconds(100));
                //if (id == null || id.Length == 0 || !_running) //?? not sure on this
                //    continue;
                ////if (id.Length != 16)
                ////    throw new Exception("We don't have a sender id for the request");
                //byte[] msgId = _socket.Receive(TimeSpan.FromSeconds(1));
                //if (msgId == null || msgId.Length != 16)
                //    throw new Exception("We don't have a msg SenderId for this request");
                //byte[] msgBuffer = _socket.Receive(TimeSpan.FromSeconds(1));
                //if (msgBuffer == null)
                //    throw new Exception("We don't have a msg for the request");
                if (_message.Recv(_socket, _timeout))
                {
                    ProcessRequest();
                }
            }
            InternalDispose();
        }

        private void ProcessRequest()
        {
            TRequest req = _requestUnmarshaller(_message.Body);
            TReply reply = _businessLogic(req);
            byte[] data = _replyMarshaller(reply);
            _message.Body = data;
            _message.Send(_socket);
        }

        private void InternalDispose()
        {
            _socket.Dispose();
            _context.Dispose();
        }

        public void Dispose()
        {
            _running = false;
        }
    }
}