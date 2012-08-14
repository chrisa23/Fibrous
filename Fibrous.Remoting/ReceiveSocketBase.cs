namespace Fibrous.Remoting
{
    using System;
    using System.Threading;
    using CrossroadsIO;
    using Fibrous.Channels;



    public class PullSocketPort<T> : ReceiveSocketBase<T>
    {
        public PullSocketPort(Context context, string address, Func<Socket, T> msgReceiver, bool useBind = true)
            : base(context, msgReceiver)
        {
            Socket = context.CreateSocket(SocketType.PULL);
            if (useBind)
            {
                Socket.Bind(address);
            }
            else
            {
                Socket.Connect(address);
            }
            Initialize();
        }
    }

    public class SubscribeSocketPort<T> : ReceiveSocketBase<T>
    {
        public SubscribeSocketPort(Context context, string address, Func<Socket, T> msgReceiver)
            : base(context, msgReceiver)
        {
            Socket = Context.CreateSocket(SocketType.SUB);
            Socket.Connect(address);
            Initialize();
        }

        public void SubscribeAll()
        {
            Socket.SubscribeAll();
        }

        public void Subscribe(byte[] key)
        {
            Socket.Subscribe(key);
        }

        public void UnsubscribeAll()
        {
            Socket.UnsubscribeAll();
        }

        public void Unsubscribe(byte[] key)
        {
            Socket.Unsubscribe(key);
        }

        public override void Dispose()
        {
            UnsubscribeAll();
            base.Dispose();
        }
    }

    public abstract class ReceiveSocketBase<T> : ISubscriberPort<T>, IDisposable
    {
        private volatile bool _running = true;
        private readonly Func<Socket, T> _msgReceiver;
        private Thread _thread;
        private Poller _poll;
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(10);
        private readonly IChannel<T> _internalChannel = new Channel<T>();
        protected readonly Context Context;
        protected Socket Socket;

        protected ReceiveSocketBase(Context context, Func<Socket, T> msgReceiver)
        {
            _msgReceiver = msgReceiver;
            Context = context;
        }

        protected void Initialize()
        {
            Socket.ReceiveReady += SocketReceiveReady;
            _poll = new Poller(new[] { Socket }); 
            _thread = new Thread(Run) { IsBackground = true };
            _thread.Start();
        }

        private void SocketReceiveReady(object sender, SocketEventArgs e)
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