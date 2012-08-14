namespace Fibrous.Remoting
{
    using System;
    using CrossroadsIO;

    public enum SendType
    {
        Pub,
        Push
    }

    public sealed class PublisherSocket<T> : IPublisherPort<T>, IDisposable
    {
        private readonly Socket _socket;
        private readonly Action<T, Socket> _msgSender;

        //? swtich to ctor with Socket and Sender?  move setup to factory?
        public PublisherSocket(Context context, string address, Action<T, Socket> msgSender, SendType type = SendType.Pub, bool bind = true)
        {
            _msgSender = msgSender;
            if(type == SendType.Pub)
            {
                _socket = context.CreateSocket(SocketType.PUB);        
            }
            if (type == SendType.Push)
            {
                _socket = context.CreateSocket(SocketType.PUSH);
            }
            
            if (bind)
            {
                _socket.Bind(address);
            }
            else
            {
                if (type == SendType.Pub)
                    throw new ArgumentException("Publish socket must use bind");
                _socket.Connect(address);
            }
        }

        public bool Publish(T msg)
        {
            _msgSender(msg, _socket);
            return true;
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }

}