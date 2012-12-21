namespace Fibrous.Remoting
{
    using System;
    using CrossroadsIO;

    public sealed class RequestSocket<TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
    {
        private readonly IRequestChannel<TRequest, TReply> _internalChannel =
            new RequestChannel<TRequest, TReply>();
        private readonly Func<byte[], TReply> _replyUnmarshaller;
        private readonly Func<TRequest, byte[]> _requestMarshaller;
        private readonly Socket _socket;

        public RequestSocket(Context context,
                             string address,
                             Func<TRequest, byte[]> requestMarshaller,
                             Func<byte[], TReply> replyUnmarshaller)
        {
            _requestMarshaller = requestMarshaller;
            _replyUnmarshaller = replyUnmarshaller;
            _socket = context.CreateSocket(SocketType.REQ);
            _socket.Connect(address);
            _internalChannel.SetRequestHandler(StubFiber.StartNew(), InternalSendRequest);
        }

        private void InternalSendRequest(IRequest<TRequest, TReply> obj)
        {
            byte[] data = _requestMarshaller(obj.Request);
            byte[] replyData = Send(data);
            TReply reply = _replyUnmarshaller(replyData);
            obj.Reply(reply);
        }

        public void Dispose()
        {
            _socket.Dispose();
        }

        private byte[] Send(byte[] request)
        {
            try
            {
                SendStatus result = _socket.Send(request);
                if (result != SendStatus.Sent)
                    throw new Exception("Error sending message on socket");
                Message msg = _socket.ReceiveMessage(); //TODO:TimeSpan.FromSeconds(5)
                if (msg.IsEmpty)
                    return new byte[0];
                return msg[0].Buffer;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new byte[0];
            }
        }

        public IDisposable SendRequest(TRequest request, Fiber fiber, Action<TReply> onReply)
        {
            return _internalChannel.SendRequest(request, fiber, onReply);
        }

        public IReply<TReply> SendRequest(TRequest request)
        {
            return _internalChannel.SendRequest(request);
        }
    }
}