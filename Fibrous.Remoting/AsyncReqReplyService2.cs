namespace Fibrous.Remoting
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using CrossroadsIO;

    public class AsyncReqReplyService2<TRequest, TReply> : IDisposable
    {
        private readonly Func<byte[], int, TRequest> _requestUnmarshaller;
        private readonly Func<TRequest, TReply> _businessLogic;
        private readonly Func<TReply, byte[]> _replyMarshaller;
        private readonly Context _context;
        private readonly Socket _socket;
        private volatile bool _running = true;

        private readonly byte[] _buffer = new byte[1024 * 1024 * 2];
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

        public AsyncReqReplyService2(string address,
                                     Func<byte[], int, TRequest> requestUnmarshaller,
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

        private void ProcessRequest(int length)
        {
            //buffer has Id right now...
            //length should be 16
           // Debug.Assert(length == 16);
            Debug.Assert(_socket.ReceiveMore);
            byte[] guidBytes = new byte[length];
            Buffer.BlockCopy(_buffer, 0, guidBytes, 0, length);

            int bodyLength = _socket.Receive(_buffer);

            TRequest request = _requestUnmarshaller(_buffer, length);
            var guid = new Guid(guidBytes);
            TReply reply = _businessLogic(request);
            byte[] replyData = _replyMarshaller(reply);
            _socket.SendMore(guidBytes);
            _socket.Send(replyData);
        }

        private void Run()
        {
            while (_running)
            {
                int length = 0;
                if ((length = _socket.Receive(_buffer, _timeout)) != -1)
                {
                    ProcessRequest(length);
                }
            }
            InternalDispose();
        }

        //private void ProcessRequest()
        //{
        //    TRequest req = _requestUnmarshaller(_message.Body);
        //    TReply reply = _businessLogic(req);
        //    byte[] data = _replyMarshaller(reply);
        //    _message.Body = data;
        //    _message.Send(_socket);
        //}

        private void InternalDispose()
        {
            _socket.Dispose();
            _context.Dispose();
        }

        public void Dispose()
        {
            _running = false;
            InternalDispose();
        }
    }
}