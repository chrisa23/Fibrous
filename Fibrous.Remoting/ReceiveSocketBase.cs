namespace Fibrous.Remoting
{
    using System;
    using System.Threading.Tasks;
    using CrossroadsIO;
    using Fibrous.Channels;

    public abstract class ReceiveSocketBase<T> : ISubscribePort<T>, IDisposable
    {
        protected readonly Context Context;
        private readonly IChannel<T> _internalChannel = new Channel<T>();
        private readonly Func<byte[], T> _msgReceiver;
        protected Socket Socket;
        private volatile bool _running = true;
        private Task _task;
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

        protected ReceiveSocketBase(Context context, Func<byte[], T> receiver)
        {
            _msgReceiver = receiver;
            Context = context;
        }

        public virtual void Dispose()
        {
            _running = false;
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            return _internalChannel.Subscribe(fiber, receive);
        }

        protected void Initialize()
        {
            _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            while (_running)
            {
                try
                {
                    Message message = Socket.ReceiveMessage();//_timeout
                    if (message.IsEmpty)
                        continue;
                    T msg = _msgReceiver(message[0].Buffer);
                    _internalChannel.Publish(msg);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _running = false;
                }
            }
            InternalDispose();
        }

        private void InternalDispose()
        {
            Socket.Dispose();
        }
    }
}