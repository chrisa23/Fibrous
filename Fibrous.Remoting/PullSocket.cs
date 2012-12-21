namespace Fibrous.Remoting
{
    using System;
    using CrossroadsIO;

    public class PullSocket<T> : ReceiveSocketBase<T>
    {
        public PullSocket(Context context, string address, Func<byte[], T> msgReceiver, IPublisherPort<T> output, bool useBind = true)
            : base(context, msgReceiver, output)
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