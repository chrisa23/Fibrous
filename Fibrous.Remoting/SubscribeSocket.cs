namespace Fibrous.Remoting
{
    using System;
    using NetMQ;
    using NetMQ.zmq;

    public sealed class SubscribeSocket<T> : ReceiveSocketBase<T>
    {
        public SubscribeSocket(NetMQContext context, string address, Func<byte[], T> msgReceiver, IPublisherPort<T> output)
            : base(context, msgReceiver, output)
        {
            Socket = Context.CreateSocket(ZmqSocketType.Sub);
            Socket.Connect(address);
            Initialize();
        }

        public void SubscribeAll()
        {
            Socket.Subscribe(new byte[0]);
        }

        public void Subscribe(byte[] key)
        {
            Socket.Subscribe(key);
        }

        public void UnsubscribeAll()
        {
            Socket.Unsubscribe(new byte[0]);
        }

        public void Unsubscribe(byte[] key)
        {
            Socket.Unsubscribe(key);
        }

        public override void Dispose()
        {
            UnsubscribeAll();
            base.Dispose();
        }
    }
}