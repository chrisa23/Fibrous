using System;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public class DealerSocketPort<T> : ReceiveSocketBase<T>
    {
        public DealerSocketPort(ZmqContext context, string address, Func<ZmqSocket, T> msgReceiver,
                                bool useBind = false)
            : base(context, msgReceiver)
        {
            Socket = context.CreateSocket(SocketType.DEALER);
            if (useBind)
                Socket.Bind(address);
            else
                Socket.Connect(address);

            Initialize();
        }
    }
}