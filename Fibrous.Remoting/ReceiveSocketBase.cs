namespace Fibrous.Remoting
{
    using System;
    using System.Threading.Tasks;
    using NetMQ;

    public abstract class ReceiveSocketBase<T> : IDisposable
    {
        protected readonly NetMQContext Context;
        private readonly Func<byte[], T> _msgReceiver;
        private readonly IPublisherPort<T> _output;
        protected NetMQSocket Socket;
        private volatile bool _running = true;

        protected ReceiveSocketBase(NetMQContext context, Func<byte[], T> receiver, IPublisherPort<T> output)
        {
            _msgReceiver = receiver;
            _output = output;
            Context = context;
        }

        public virtual void Dispose()
        {
            Socket.Close();
        }

        protected void Initialize()
        {
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            while (_running)
            {
                NetMQMessage message;
                try
                {
                    message = Socket.ReceiveMessage(); //_timeout
                }
                catch (Exception e)
                {
                    //This needs improvement...
                    Console.WriteLine(e);
                    _running = false;
                    break;
                }
                if (message.IsEmpty)
                    continue;
                T msg = _msgReceiver(message[0].Buffer);
                _output.Publish(msg);
            }
            InternalDispose();
        }

        private void InternalDispose()
        {
            Socket.Dispose();
        }
    }
}