namespace Fibrous.Zmq
{
    using System;
    using ZeroMQ;

    public class SubscribeSocketPort<T> : ReceiveSocketBase<T>
    {
        public SubscribeSocketPort(IZmqContext context, string address, Func<IReceiveSocket, T> msgReceiver)
            : base(context, msgReceiver)
        {
            Socket = Context.CreateSubscribeSocket();
            Socket.Connect(address);
            Initialize();
        }

        public void SubscribeAll()
        {
            ((ISubscribeSocket)Socket).SubscribeAll();
        }

        public void Subscribe(byte[] key)
        {
            ((ISubscribeSocket)Socket).Subscribe(key);
        }

        public void UnsubscribeAll()
        {
            ((ISubscribeSocket)Socket).UnsubscribeAll();
        }

        public void Unsubscribe(byte[] key)
        {
            ((ISubscribeSocket)Socket).Unsubscribe(key);
        }

        public override void Dispose()
        {
            UnsubscribeAll();
            base.Dispose();
        }
    }
}