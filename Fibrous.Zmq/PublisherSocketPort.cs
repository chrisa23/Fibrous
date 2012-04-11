namespace Fibrous.Zmq
{
    using System;
    using ZeroMQ;

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