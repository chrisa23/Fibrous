using System;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public sealed class PublisherSocketPort<T> : SendSocketBase<T>
    {
        public PublisherSocketPort(IZmqContext context, string address, Action<T, ISendSocket> msgSender)
            : base(msgSender)
        {
            Socket = context.CreatePublishSocket();
            Socket.Bind(address);
        }
    }
}