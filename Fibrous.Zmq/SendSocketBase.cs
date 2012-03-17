using System;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public class SendSocketBase<T> : IPublisherPort<T>, IDisposable
    {
        protected ZmqSocket Socket;
        private readonly Action<T, ZmqSocket> _msgSender;

        protected SendSocketBase(Action<T, ZmqSocket> msgSender)
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