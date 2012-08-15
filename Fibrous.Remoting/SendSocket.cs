namespace Fibrous.Remoting
{
    using System;
    using CrossroadsIO;

    public sealed class SendSocket<T> : IPublisherPort<T>, IDisposable
    {
        private readonly Socket _socket;
        private readonly Action<T, Socket> _msgSender;
        //? swtich to ctor with Socket and Sender?  move setup to factory?
        public SendSocket(Context context,
                          string address,
                          Action<T, Socket> msgSender,
                          SocketType type = SocketType.PUB,
                          bool bind = true)
        {
            _msgSender = msgSender;
            _socket = context.CreateSocket(type);
            if (bind)
            {
                _socket.Bind(address);
            }
            else
            {
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

        public static SendSocket<A> NewPubSocket<A>(Context context, string address, Action<A, Socket> msgSender)
        {
            return new SendSocket<A>(context, address, msgSender);
        }

        public static SendSocket<A> NewPushSocket<A>(Context context,
                                                     string address,
                                                     Action<A, Socket> msgSender,
                                                     bool bind = false)
        {
            return new SendSocket<A>(context, address, msgSender, SocketType.PUSH, bind);
        }
    }
}