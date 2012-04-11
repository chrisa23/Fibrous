namespace Fibrous.Zmq
{
    using System;
    using System.Threading;
    using Fibrous.Channels;
    using ZeroMQ;

    public abstract class ReceiveSocketBase<T> : ISubscriberPort<T>, IDisposable
    {
        private volatile bool _running = true;
        private readonly Func<IReceiveSocket, T> _msgReceiver;
        private Thread _thread;
        private IPollSet _poll;
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(10);
        private readonly IChannel<T> _internalChannel = new Channel<T>();
        protected readonly IZmqContext Context;
        protected IReceiveSocket Socket;

        protected ReceiveSocketBase(IZmqContext context, Func<IReceiveSocket, T> msgReceiver)
        {
            _msgReceiver = msgReceiver;
            Context = context;
        }

        protected void Initialize()
        {
            Socket.ReceiveReady += SocketReceiveReady;
            _poll = Context.CreatePollSet(new ISocket[] { Socket });
            _thread = new Thread(Run) { IsBackground = true };
            _thread.Start();
        }

        private void SocketReceiveReady(object sender, ReceiveReadyEventArgs e)
        {
            T msg = _msgReceiver(Socket);
            _internalChannel.Publish(msg);
        }

        private void Run()
        {
            while (_running)
            {
                _poll.Poll(_timeout);
            }
        }

        public virtual void Dispose()
        {
            _running = false;
            if (!_thread.Join(200))
            {
                _thread.Abort();
            }
            _poll.Dispose();
            Socket.Dispose();
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            return _internalChannel.Subscribe(fiber, receive);
        }
    }
}