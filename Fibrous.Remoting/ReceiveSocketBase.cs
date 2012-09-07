namespace Fibrous.Remoting
{
    using System;
    using System.Threading.Tasks;
    using CrossroadsIO;
    using Fibrous.Channels;

    public class PullSocketPort<T> : ReceiveSocketBase<T>
    {
        public PullSocketPort(Context context, string address, Func<byte[], int, T> msgReceiver, bool useBind = true)
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
        public SubscribeSocketPort(Context context, string address, Func<byte[], int, T> msgReceiver)
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

    public abstract class ReceiveSocketBase<T> : ISubscribePort<T>, IDisposable
    {
        private volatile bool _running = true;
        private readonly Func<byte[], int, T> _msgReceiver;
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);
        private readonly IChannel<T> _internalChannel = new Channel<T>();
        protected readonly Context Context;
        protected Socket Socket;

        protected ReceiveSocketBase(Context context, Func<byte[], int, T> receiver)
        {
            _msgReceiver = receiver;
            Context = context;
        }

        protected void Initialize()
        {
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            while (_running)
            {
                Message message = Socket.ReceiveMessage(); //_timeout);
                if (message.IsEmpty)
                {
                    continue;
                }
                T msg = _msgReceiver(message[0].Buffer, message[0].BufferSize);
                _internalChannel.Publish(msg);
            }
            InternalDispose();
        }

        private void InternalDispose()
        {
            Socket.Dispose();
        }

        public virtual void Dispose()
        {
            _running = false;
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            return _internalChannel.Subscribe(fiber, receive);
        }
    }
}