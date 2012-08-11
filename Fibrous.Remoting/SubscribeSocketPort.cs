namespace Fibrous.Remoting
{
    using System;
    using CrossroadsIO;

    public class SubscribeSocketPort<T> : ReceiveSocketBase<T>
    {
        public SubscribeSocketPort(Context context, string address, Func<Socket, T> msgReceiver)
            : base(context, msgReceiver)
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