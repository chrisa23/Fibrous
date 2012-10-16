using System;
using System.Threading.Tasks;
using CrossroadsIO;
using Fibrous.Fibers;

namespace Fibrous.Remoting
{
    public class AsyncRequestService<TRequest, TReply> : IDisposable
    {
        private readonly IAsyncRequestPort<TRequest, TReply> _businessLogic;
        //split InSocket
        private readonly Context _context;
        private readonly IFiber _fiber = PoolFiber.StartNew();
        private readonly Func<TReply, byte[]> _replyMarshaller;
        //split OutSocket
        private readonly Socket _replySocket;
        private readonly Socket _requestSocket;
        private readonly Func<byte[], TRequest> _requestUnmarshaller;
        private volatile bool _running = true;

        public AsyncRequestService(Context context,
                                   string address,
                                   int basePort,
                                   Func<byte[], TRequest> requestUnmarshaller,
                                   IAsyncRequestPort<TRequest, TReply> logic,
                                   Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _businessLogic = logic;
            _replyMarshaller = replyMarshaller;
            _context = context;
            _requestSocket = _context.CreateSocket(SocketType.PULL);
            _requestSocket.Bind(address + ":" + basePort);
            _replySocket = _context.CreateSocket(SocketType.PUB);
            _replySocket.Bind(address + ":" + (basePort + 1));
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _running = false;
        }

        #endregion

        private void Run()
        {
            while (_running)
            {
                Message message = _requestSocket.ReceiveMessage();
                if (message.IsEmpty)
                    continue;
                byte[] id = message[0].Buffer;
                byte[] rid = message[1].Buffer;
                ProcessRequest(id, rid, message[2].Buffer);
            }
            InternalDispose();
        }

        private void ProcessRequest(byte[] id, byte[] msgId, byte[] msgBuffer)
        {
            TRequest req = _requestUnmarshaller(msgBuffer);
            _businessLogic.SendRequest(req, _fiber, reply => SendReply(id, msgId, reply));
        }

        private void SendReply(byte[] id, byte[] msgId, TReply reply)
        {
            byte[] data = _replyMarshaller(reply);
            _replySocket.SendMore(id);
            _replySocket.SendMore(msgId);
            _replySocket.Send(data);
        }

        private void InternalDispose()
        {
            _requestSocket.Dispose();
            _replySocket.Dispose();
        }
    }
}