namespace Fibrous.Zmq
{
    using System;
    using ZeroMQ;

    public class SendSocketBase<T> : ISenderPort<T>, IDisposable
    {
        protected ZmqSocket Socket;
        private readonly Action<T, ZmqSocket> _msgSender;

        protected SendSocketBase(Action<T, ZmqSocket> msgSender)
        {
            _msgSender = msgSender;
        }

        public bool Send(T msg)
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