namespace Fibrous.Remoting
{
    //public class RemoteRequestPort<TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
    //{
    //    private readonly Func<byte[], TReply> _replyUnmarshaller;
    //    private readonly Func<TRequest, byte[]> _requestMarshaller;
    //    private readonly Socket _socket;
    //    public RemoteRequestPort(Context context,
    //                             string address,
    //                             Func<TRequest, byte[]> requestMarshaller,
    //                             Func<byte[], TReply> replyUnmarshaller)
    //    {
    //        _requestMarshaller = requestMarshaller;
    //        _replyUnmarshaller = replyUnmarshaller;
    //        _socket = context.CreateSocket(SocketType.REQ);
    //        _socket.Connect(address);
    //    }
    //    public void Dispose()
    //    {
    //        _socket.Dispose();
    //    }
    //    public TReply SendRequest(TRequest request, TimeSpan timeout)
    //    {
    //        byte[] data = _requestMarshaller(request);
    //        byte[] replyData = Send(data, timeout);
    //        TReply reply = _replyUnmarshaller(replyData);
    //        return reply;
    //    }
    //    private byte[] Send(byte[] request, TimeSpan timeout)
    //    {
    //        SendStatus result = _socket.Send(request);
    //        if (result != SendStatus.Sent)
    //            throw new Exception("Error sending message on socket");
    //        Message msg = _socket.ReceiveMessage(timeout);
    //        if (msg.IsEmpty)
    //            return new byte[0];
    //        return msg[0].Buffer;
    //    }
    //    public IDisposable SendRequest(TRequest request, Fiber fiber, Action<TReply> onReply)
    //    {
    //    }
    //    public IReply<TReply> SendRequest(TRequest request)
    //    {
    //    }
    //}
}