namespace Fibrous.Zmq
{
    using System;
    using ZeroMQ;

    public class DealerSocketPort<T> : ReceiveSocketBase<T>
    {
        public DealerSocketPort(IZmqContext context,
                                string address,
                                Func<IReceiveSocket, T> msgReceiver,
                                bool useBind = false)
            : base(context, msgReceiver)
        {
            Socket = context.CreateDealerSocket();
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