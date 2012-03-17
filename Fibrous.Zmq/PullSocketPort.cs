using System;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public class PullSocketPort<T> : ReceiveSocketBase<T>
    {
        public PullSocketPort(ZmqContext context, string address, Func<ZmqSocket, T> msgReceiver,
                              bool useBind = true)
            : base(context, msgReceiver)
        {
            Socket = context.CreateSocket(SocketType.PULL);
            if (useBind)
                Socket.Bind(address);
            else
                Socket.Connect(address);

            Initialize();
        }
    }
}