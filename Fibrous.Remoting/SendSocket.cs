using System;
using CrossroadsIO;

namespace Fibrous.Remoting
{
    public sealed class SendSocket<T> : IPublishPort<T>, IDisposable
    {
        private readonly Func<T, byte[]> _msgSender;
        private readonly Socket _socket;
        //? swtich to ctor with Socket and Sender?  move setup to factory?
        public SendSocket(Context context,
                          string address,
                          Func<T, byte[]> marshaller,
                          SocketType type = SocketType.PUB,
                          bool bind = true)
        {
            _msgSender = marshaller;
            _socket = context.CreateSocket(type);
            if (bind)
                _socket.Bind(address);
            else
                _socket.Connect(address);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _socket.Dispose();
        }

        #endregion

        #region IPublishPort<T> Members

        public bool Publish(T msg)
        {
            //intended to allow multipart messages.. but inconsistent'
            _socket.Send(_msgSender(msg));
            return true;
        }

        #endregion

        public static SendSocket<A> NewPubSocket<A>(Context context, string address, Func<A, byte[]> msgSender)
        {
            return new SendSocket<A>(context, address, msgSender);
        }

        public static SendSocket<A> NewPushSocket<A>(Context context,
                                                     string address,
                                                     Func<A, byte[]> msgSender,
                                                     bool bind = false)
        {
            return new SendSocket<A>(context, address, msgSender, SocketType.PUSH, bind);
        }
    }
}