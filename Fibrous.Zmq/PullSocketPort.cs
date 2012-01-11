using System;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public class PullSocketPort<T> : ReceiveSocketBase<T>
    {
        public PullSocketPort(IZmqContext context, string address, Func<IReceiveSocket, T> msgReceiver,
                              bool useBind = true)
            : base(context, msgReceiver)
        {
            Socket = context.CreatePullSocket();
            if (useBind)
                Socket.Bind(address);
            else
                Socket.Connect(address);

            Initialize();
        }
    }
}