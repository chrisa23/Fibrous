namespace Fibrous.Remoting
{
    using System;
    using System.Threading.Tasks;
    using CrossroadsIO;
    using Fibrous.Channels;

    public class RemoteRequestHandlerPort<TRequest, TReply> : IRequestHandlerPort<TRequest, TReply>, IDisposable
    {
        private readonly IRequestChannel<TRequest, TReply> _internalChannel = new RequestChannel<TRequest, TReply>();
        private readonly Func<TReply, byte[]> _replyMarshaller;
        private readonly Func<byte[], TRequest> _requestUnmarshaller;
        private readonly Socket _socket;
        private readonly TimeSpan _timeout;
        private bool _running = true;

        public RemoteRequestHandlerPort(Context context,
                                        string address,
                                        Func<byte[], TRequest> requestUnmarshaller,
                                        Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _replyMarshaller = replyMarshaller;
            _timeout = TimeSpan.FromMilliseconds(100);
            _socket = context.CreateSocket(SocketType.REP);
            _socket.Bind(address);
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            _socket.Dispose();
        }

        private void ProcessRequest(byte[] buffer)
        {
            TRequest request = _requestUnmarshaller(buffer);
            TReply reply = _internalChannel.SendRequest(request, TimeSpan.FromMinutes(5)); //??
            byte[] replyData = _replyMarshaller(reply);
            _socket.Send(replyData, _timeout);
        }

        private void Run()
        {
            while (_running)
            {
                //check for time/cutoffs to trigger events...
                try
                {
                    Message msg = _socket.ReceiveMessage();
                    if (msg.IsEmpty)
                        continue;
                    //copy so we aren't using a callback to an updated Id or rId buffer
                    ProcessRequest(msg[0].Buffer);
                }
                catch (Exception e)
                {
                    _running = false;
                }
            }
          //  InternalDispose();
        }

        //private void InternalDispose()
        //{
          
        //}

        public IDisposable SetRequestHandler(IFiber fiber, Action<IRequest<TRequest, TReply>> onRequest)
        {
            return _internalChannel.SetRequestHandler(fiber, onRequest);
        }
    }
}