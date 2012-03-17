namespace Fibrous.Zmq
{
    using System;
    using ZeroMQ;
    
    public sealed class PushSocketPort<T> : SendSocketBase<T>
    {
        public PushSocketPort(ZmqContext context, string address, Action<T, ZmqSocket> msgSender, bool bind = false)
            : base(msgSender)
        {
            Socket = context.CreateSocket(SocketType.PUSH);

            if (bind)
                Socket.Bind(address);
            else
                Socket.Connect(address);
        }
    }
}