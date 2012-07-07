namespace Fibrous.Zmq
{
    using System;
    using CrossroadsIO;

    public sealed class PublisherSocketPort<T> : SendSocketBase<T>
    {
        public PublisherSocketPort(Context context, string address, Action<T, Socket> msgSender)
            : base(msgSender)
        {
            Socket = context.CreateSocket(SocketType.PUB);
            Socket.Bind(address);
        }
    }
}