namespace Fibrous.Zmq
{
    using System;
    using ZeroMQ;

    public class PullSocketPort<T> : ReceiveSocketBase<T>
    {
        public PullSocketPort(ZmqContext context, string address, Func<ZmqSocket, T> msgReceiver, bool useBind = true)
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