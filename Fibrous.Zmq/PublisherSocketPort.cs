using System;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public sealed class PublisherSocketPort<T> : SendSocketBase<T>
    {
        public PublisherSocketPort(ZmqContext context, string address, Action<T, ZmqSocket> msgSender)
            : base(msgSender)
        {
            Socket = context.CreateSocket(SocketType.PUB);
            Socket.Bind(address);
        }
    }
}