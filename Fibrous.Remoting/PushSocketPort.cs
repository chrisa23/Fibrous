namespace Fibrous.Zmq
{
    using System;
    using CrossroadsIO;

    public sealed class PushSocketPort<T> : SendSocketBase<T>
    {
        public PushSocketPort(Context context, string address, Action<T, Socket> msgSender, bool bind = false)
            : base(msgSender)
        {
            Socket = context.CreateSocket(SocketType.PUSH);
            if (bind)
            {
                Socket.Bind(address);
            }
            else
            {
                Socket.Connect(address);
            }
        }
    }
}