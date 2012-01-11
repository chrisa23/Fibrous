using System;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public class DealerSocketPort<T> : ReceiveSocketBase<T>
    {
        public DealerSocketPort(IZmqContext context, string address, Func<IReceiveSocket, T> msgReceiver,
                                bool useBind = false)
            : base(context, msgReceiver)
        {
            Socket = context.CreateDealerSocket();
            if (useBind)
                Socket.Bind(address);
            else
                Socket.Connect(address);

            Initialize();
        }
    }
}