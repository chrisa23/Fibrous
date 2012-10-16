using System;
using System.Threading.Tasks;
using CrossroadsIO;

namespace Fibrous.Remoting
{
    public class RequestService<TRequest, TReply> : IDisposable
    {
        private readonly IRequestPort<TRequest, TReply> _businessLogic;
        private readonly Func<TReply, byte[]> _replyMarshaller;
        private readonly Func<byte[], TRequest> _requestUnmarshaller;
        private readonly Socket _socket;
        private readonly TimeSpan _timeout;
        private bool _running = true;

        public RequestService(Context context,
                              string address,
                              Func<byte[], TRequest> requestUnmarshaller,
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

        #region IDisposable Members

        public void Dispose()
        {
            _running = false;
        }

        #endregion

        private void ProcessRequest(byte[] buffer)
        {
            TRequest request = _requestUnmarshaller(buffer);
            TReply reply = _businessLogic.SendRequest(request, TimeSpan.FromDays(1)); //??
            byte[] replyData = _replyMarshaller(reply);
            _socket.Send(replyData, _timeout);
        }

        private void Run()
        {
            while (_running)
            {
                //check for time/cutoffs to trigger events...
                Message msg = _socket.ReceiveMessage(_timeout);
                if (msg.IsEmpty)
                    continue;
                //copy so we aren't using a callback to an updated Id or rId buffer
                ProcessRequest(msg[0].Buffer);
            }
            InternalDispose();
        }

        private void InternalDispose()
        {
            _socket.Dispose();
        }
    }
}