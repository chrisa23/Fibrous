namespace Fibrous.Remoting
{
    using System;
    using CrossroadsIO;

    public class SubscribeSocket<T> : ReceiveSocketBase<T>
    {
        public SubscribeSocket(Context context, string address, Func<byte[], T> msgReceiver, IPublisherPort<T> output)
            : base(context, msgReceiver, output)
        {
            Socket = Context.CreateSocket(SocketType.SUB);
            Socket.Connect(address);
            Initialize();
        }

        public void SubscribeAll()
        {
            Socket.SubscribeAll();
        }

        public void Subscribe(byte[] key)
        {
            Socket.Subscribe(key);
        }

        public void UnsubscribeAll()
        {
            Socket.UnsubscribeAll();
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