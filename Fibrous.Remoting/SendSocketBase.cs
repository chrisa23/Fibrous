namespace Fibrous.Zmq
{
    using System;
    using CrossroadsIO;

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
}