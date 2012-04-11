namespace Fibrous.Zmq
{
    using System;
    using ZeroMQ;

    public class ReqReplyClient<TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
    {
        private readonly Func<TRequest, byte[]> _requestMarshaller;
        private readonly Func<byte[], TReply> _replyUnmarshaller;
        private readonly IDuplexSocket _socket;

        public ReqReplyClient(IZmqContext context,
                              string address,
                              Func<TRequest, byte[]> requestMarshaller,
                              Func<byte[], TReply> replyUnmarshaller)
        {
            _requestMarshaller = requestMarshaller;
            _replyUnmarshaller = replyUnmarshaller;
            _socket = context.CreateRequestSocket();
            _socket.Connect(address);
        }

        private byte[] Send(byte[] request, TimeSpan timeout)
        {
            SendResult result = _socket.Send(request);
            while (result == SendResult.TryAgain)
            {
                result = _socket.Send(request);
            }
            if (result != SendResult.Sent)
            {
                throw new Exception("Error sending message on socket");
            }
            return _socket.Receive(timeout);
        }

        public void Dispose()
        {
            _socket.Dispose();
        }

        public TReply SendRequest(TRequest request, TimeSpan timeout)
        {
            byte[] data = _requestMarshaller(request);
            byte[] replyData = Send(data, timeout);
            TReply reply = _replyUnmarshaller(replyData);
            return reply;
        }
    }
}