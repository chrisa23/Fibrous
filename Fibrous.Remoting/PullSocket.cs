namespace Fibrous.Remoting
{
    using System;
    
    using NetMQ;
    using NetMQ.zmq;

    public sealed class PullSocket<T> : ReceiveSocketBase<T>
    {
        public PullSocket(NetMQContext context, string address, Func<byte[], T> msgReceiver, IPublisherPort<T> output, bool useBind = true)
            : base(context, msgReceiver, output)
        {
            Socket = context.CreateSocket(ZmqSocketType.Pull);
            if (useBind)
                Socket.Bind(address);
            else
                Socket.Connect(address);
            Initialize();
        }
    }
}