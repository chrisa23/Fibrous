namespace Fibrous.Remoting
{
    using System;
    using CrossroadsIO;

    public sealed class PublisherSocketPort<T> : SendSocketBase<T>
    {
        public PublisherSocketPort(Context context, string address, Action<T, Socket> msgSender)
            : base(msgSender)
        {
            Socket = context.CreateSocket(SocketType.PUB);
            Socket.Bind(address);
        }
    }

    public class SendSocketBase<T> : IPublisherPort<T>, IDisposable
    {
        protected Socket Socket;
        private readonly Action<T, Socket> _msgSender;

        protected SendSocketBase(Action<T, Socket> msgSender)
        {
            _msgSender = msgSender;
        }

        public bool Publish(T msg)
        {
            _msgSender(msg, Socket);
            return true;
        }

        public void Dispose()
        {
            Socket.Dispose();
        }
    }

    public sealed class PushSocketPort<T> : SendSocketBase<T>
    {
        public PushSocketPort(Context context, string address, Action<T, Socket> msgSender, bool bind = false)
            : base(msgSender)
        {
            Socket = context.CreateSocket(SocketType.PUSH);
            if (bind)
            {
                Socket.Bind(address);
            }
            else
            {
                Socket.Connect(address);
            }
        }
    }
}