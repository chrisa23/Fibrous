namespace Fibrous.Zmq
{
    using System;
    using ZeroMQ;

    public class ReqReplyClient<TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
    {
        private readonly Func<TRequest, byte[]> _requestMarshaller;
        private readonly Func<byte[], int, TReply> _replyUnmarshaller;
        private readonly ZmqSocket _socket;
        private readonly byte[] _buffer = new byte[1024 * 1024 * 2];

        public ReqReplyClient(ZmqContext context,
                              string address,
                              Func<TRequest, byte[]> requestMarshaller,
                              Func<byte[], int, TReply> replyUnmarshaller)
        {
            _requestMarshaller = requestMarshaller;
            _replyUnmarshaller = replyUnmarshaller;
            _socket = context.CreateSocket(SocketType.REQ);
            _socket.Connect(address);
        }

        private byte[] Send(byte[] request, TimeSpan timeout)
        {
            SendStatus result = _socket.Send(request);
            while (result == SendStatus.TryAgain)
            {
                result = _socket.Send(request);
            }
            if (result != SendStatus.Sent)
            {
                throw new Exception("Error sending message on socket");
            }
            int length = _socket.Receive(_buffer, timeout);
            var reply = new byte[length];
            Array.Copy(_buffer, reply, length);
            return reply;
        }

        public void Dispose()
        {
            _socket.Dispose();
        }

        public TReply SendRequest(TRequest request, TimeSpan timeout)
        {
            byte[] data = _requestMarshaller(request);
            byte[] replyData = Send(data, timeout);
            TReply reply = _replyUnmarshaller(replyData, replyData.Length);
            return reply;
        }
    }
}