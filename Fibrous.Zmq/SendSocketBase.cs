using System;
using ZeroMQ;

namespace Fibrous.Zmq
{
    public class SendSocketBase<T> : IPublisherPort<T>, IDisposable
    {
        protected ISendSocket Socket;
        private readonly Action<T, ISendSocket> _msgSender;

        protected SendSocketBase(Action<T, ISendSocket> msgSender)
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