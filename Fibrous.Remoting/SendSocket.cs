namespace Fibrous.Remoting
{
    using System;
    using NetMQ;
    using NetMQ.zmq;

    public sealed class SendSocket<T> : IPublisherPort<T>, IDisposable
    {
        private readonly Func<T, byte[]> _msgSender;
        private readonly NetMQSocket _socket;
        //? swtich to ctor with Socket and Sender?  move setup to factory?
        public SendSocket(NetMQContext context,
                          string address,
                          Func<T, byte[]> marshaller,
                          ZmqSocketType type = ZmqSocketType.Pub,
                          bool bind = true)
        {
            _msgSender = marshaller;
            _socket = context.CreateSocket(type);
            if (bind)
                _socket.Bind(address);
            else
                _socket.Connect(address);
        }

        public void Dispose()
        {
            //_socket.Close();
            _socket.Dispose();
        }

        public bool Publish(T msg)
        {
            _socket.Send(_msgSender(msg));
            return true;
        }

        public static SendSocket<A> NewPubSocket<A>(NetMQContext context, string address, Func<A, byte[]> msgSender)
        {
            return new SendSocket<A>(context, address, msgSender);
        }

        public static SendSocket<A> NewPushSocket<A>(NetMQContext context,
                                                     string address,
                                                     Func<A, byte[]> msgSender,
                                                     bool bind = false)
        {
            return new SendSocket<A>(context, address, msgSender, ZmqSocketType.Push, bind);
        }
    }
}