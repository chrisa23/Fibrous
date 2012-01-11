using System;
using System.Threading;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public class ReqReplyService<TRequest, TReply> : IDisposable
    {
        private readonly Func<byte[], TRequest> _requestUnmarshaller;
        private readonly Func<TRequest, TReply> _businessLogic;
        private readonly Func<TReply, byte[]> _replyMarshaller;
        private bool _running = true;

        private readonly IDuplexSocket _socket;
        private readonly Thread _thread;
        private readonly IPollSet _poll;

        private readonly TimeSpan _timeout;


        public ReqReplyService(IZmqContext context,
                               string address,
                               Func<byte[], TRequest> requestUnmarshaller,
                               Func<TRequest, TReply> businessLogic,
                               Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _businessLogic = businessLogic;
            _replyMarshaller = replyMarshaller;
            _timeout = TimeSpan.FromMilliseconds(100);

            _socket = context.CreateReplySocket();
            _socket.Bind(address);
            _socket.ReceiveReady += SocketReceiveReady;

            _poll = context.CreatePollSet(new ISocket[] {_socket});

            _thread = new Thread(Run) {IsBackground = true};
            _thread.Start();
        }

        private void SocketReceiveReady(object sender, ReceiveReadyEventArgs e)
        {
            byte[] requestData = _socket.Receive();
            TRequest request = _requestUnmarshaller(requestData);
            TReply reply = _businessLogic(request);
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