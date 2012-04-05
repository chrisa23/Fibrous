namespace Fibrous.Zmq
{
    using System;
    using ZeroMQ;

    public class DealerSocketPort<T> : ReceiveSocketBase<T>
    {
        public DealerSocketPort(ZmqContext context, string address, Func<ZmqSocket, T> msgReceiver, bool useBind = false)
            : base(context, msgReceiver)
        {
            Socket = context.CreateSocket(SocketType.DEALER);
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