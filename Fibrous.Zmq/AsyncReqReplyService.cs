using System;
using System.Threading.Tasks;
using ZeroMQ;
using ZeroMQ.Sockets;

namespace Fibrous.Zmq
{
    public class AsyncReqReplyService<TRequest, TReply> : IDisposable
    {
        private readonly Func<byte[], TRequest> _requestUnmarshaller;
        private readonly Func<TRequest, TReply> _businessLogic;
        private readonly Func<TReply, byte[]> _replyMarshaller;
        //split InSocket
        private readonly IZmqContext _context;
        private readonly IReceiveSocket _requestSocket;

        //split OutSocket
        private readonly ISendSocket _replySocket;

        private volatile bool _running = true;

        public AsyncReqReplyService(string requestAddress,
                                    string replyAddress,
                                    Func<byte[], TRequest> requestUnmarshaller,
                                    Func<TRequest, TReply> businessLogic,
                                    Func<TReply, byte[]> replyMarshaller)
        {
            _requestUnmarshaller = requestUnmarshaller;
            _businessLogic = businessLogic;
            _replyMarshaller = replyMarshaller;

            _context = ZmqContext.Create();
            _requestSocket = _context.CreatePullSocket();
            _requestSocket.Bind(requestAddress);

            _replySocket = _context.CreatePublishSocket();
            _replySocket.Bind(replyAddress);

            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            while (_running)
            {
                //check for time/cutoffs to trigger events...

                byte[] id = _requestSocket.Receive(TimeSpan.FromMilliseconds(100));
                if (id == null || id.Length == 0 || !_running) //?? not sure on this
                    continue;

                if (id.Length != 16)
                    throw new Exception("We don't have a sender id for the request");

                byte[] msgId = _requestSocket.Receive(TimeSpan.FromSeconds(1));
                if (msgId == null || msgId.Length != 16)
                    throw new Exception("We don't have a msg SenderId for this request");

                byte[] msgBuffer = _requestSocket.Receive(TimeSpan.FromSeconds(1));
                if (msgBuffer == null)
                    throw new Exception("We don't have a msg for the request");

                ProcessRequest(id, msgId, msgBuffer);
            }

            InternalDispose();
        }

        private void ProcessRequest(byte[] id, byte[] msgId, byte[] msgBuffer)
        {
            var req = _requestUnmarshaller(msgBuffer);
            var reply = _businessLogic(req);
            SendReply(id, msgId, reply);
        }

        private void SendReply(byte[] id, byte[] msgId, TReply reply)
        {
            byte[] data = _replyMarshaller(reply);
            _replySocket.SendPart(id);
            _replySocket.SendPart(msgId);
            _replySocket.Send(data);
        }

        private void InternalDispose()
        {
            _requestSocket.Dispose();
            _replySocket.Dispose();
            _context.Dispose();
        }

        public void Dispose()
        {
            _running = false;
        }
    }
}