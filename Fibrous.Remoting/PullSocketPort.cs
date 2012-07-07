namespace Fibrous.Zmq
{
    using System;
    using CrossroadsIO;

    public class PullSocketPort<T> : ReceiveSocketBase<T>
    {
        public PullSocketPort(Context context, string address, Func<Socket, T> msgReceiver, bool useBind = true)
            : base(context, msgReceiver)
        {
            Socket = context.CreateSocket(SocketType.PULL);
            if (useBind)
            {
                Socket.Bind(address);
            }
            else
            {
                Socket.Connect(address);
            }
            Initialize();
        }
    }
}