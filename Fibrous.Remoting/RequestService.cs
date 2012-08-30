namespace Fibrous.Remoting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CrossroadsIO;

    public class RequestService<TRequest, TReply> : IDisposable
    {
        private readonly Func<byte[], int, TRequest> _requestUnmarshaller;
        private readonly IRequestPort<TRequest, TReply> _businessLogic;
        private readonly Func<TReply, byte[]> _replyMarshaller;
        private bool _running = true;
        private readonly Socket _socket;
        private readonly TimeSpan _timeout;

        public RequestService(Context context,
                              string address,
                              Func<byte[], int, TRequest> requestUnmarshaller,
                              IRequestPort<TRequest, TReply> businessLogic,
                              Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _businessLogic = businessLogic;
            _replyMarshaller = replyMarshaller;
            _timeout = TimeSpan.FromMilliseconds(100);
            _socket = context.CreateSocket(SocketType.REP);
            _socket.Bind(address);
            
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void ProcessRequest(byte[] buffer)
        {
            TRequest request = _requestUnmarshaller(buffer, buffer.Length);
            TReply reply = _businessLogic.SendRequest(request, TimeSpan.FromDays(1)); //??
            byte[] replyData = _replyMarshaller(reply);
            _socket.Send(replyData, _timeout);
        }

        private void Run()
        {
            while (_running)
            {
                //check for time/cutoffs to trigger events...
                Message msg = _socket.ReceiveMessage( _timeout);
                if (msg.IsEmpty)
                {
                    continue;
                }
                //copy so we aren't using a callback to an updated Id or rId buffer

                ProcessRequest(msg[0].Buffer);
            }
            InternalDispose();
        }

        private void InternalDispose()
        {
            _socket.Dispose();
        }

        public void Dispose()
        {
            _running = false;
            
        }
    }
}