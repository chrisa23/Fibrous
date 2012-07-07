namespace Fibrous.Zmq
{
    using System;
    using CrossroadsIO;

    public class DealerSocketPort<T> : ReceiveSocketBase<T>
    {
        public DealerSocketPort(Context context, string address, Func<Socket, T> msgReceiver, bool useBind = false)
            : base(context, msgReceiver)
        {
            Socket = context.CreateSocket(SocketType.XREQ);
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