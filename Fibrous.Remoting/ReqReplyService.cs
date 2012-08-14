namespace Fibrous.Remoting
{
    using System;
    using System.Threading;
    using CrossroadsIO;

    public class ReqReplyService<TRequest, TReply> : IDisposable
    {
        private readonly Func<byte[], int, TRequest> _requestUnmarshaller;
        private readonly IRequestPort<TRequest, TReply> _businessLogic;
        private readonly Func<TReply, byte[]> _replyMarshaller;
        private bool _running = true;
        private readonly Socket _socket;
        private readonly Thread _thread;
        private readonly Poller _poll;
        private readonly TimeSpan _timeout;
        private readonly byte[] _buffer;

        public ReqReplyService(Context context,
                               string address,
                               Func<byte[], int, TRequest> requestUnmarshaller,
                                IRequestPort<TRequest,TReply> businessLogic,
                               Func<TReply, byte[]> replyMarshaller, int bufferSize)
        {
            _buffer = new byte[bufferSize];
            _requestUnmarshaller = requestUnmarshaller;
            _businessLogic = businessLogic;
            _replyMarshaller = replyMarshaller;
            _timeout = TimeSpan.FromMilliseconds(100);
            _socket = context.CreateSocket(SocketType.REP);
            _socket.Bind(address);
            _socket.ReceiveReady += SocketReceiveReady;
            _poll = new Poller(new[] { _socket });
            _thread = new Thread(Run) { IsBackground = true };
            _thread.Start();
        }

        private void SocketReceiveReady(object sender, SocketEventArgs e)
        {
            int requestLength = _socket.Receive(_buffer);
            TRequest request = _requestUnmarshaller(_buffer, requestLength);
            TReply reply = _businessLogic.SendRequest(request, TimeSpan.FromDays(1));//??
            byte[] replyData = _replyMarshaller(reply);
            _socket.Send(replyData);
        }

        private void Run()
        {
            while (_running)
            {
                _poll.Poll(_timeout);
            }
        }

        public void Dispose()
        {
            _running = false;
            if (!_thread.Join(200))
            {
                _thread.Abort();
            }
            _poll.Dispose();
            _socket.Dispose();
        }
    }
}