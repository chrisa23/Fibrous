namespace Fibrous.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CrossroadsIO;
    using Fibrous.Channels;
    using Fibrous.Fibers;

    public class AsyncRequestRemotingPort<TRequest, TReply> : IAsyncRequestPort<TRequest, TReply>, IDisposable
    {
        private readonly IFiber _fiber;
        private readonly byte[] _id = GetId();
        private readonly IAsyncRequestChannel<TRequest, TReply> _internalChannel =
            new AsyncRequestChannel<TRequest, TReply>();
        private readonly Context _replyContext;
        private readonly Socket _replySocket;
        private readonly Func<byte[], TReply> _replyUnmarshaller;
        private readonly Func<TRequest, byte[]> _requestMarshaller;
        private readonly Socket _requestSocket;
        private readonly Dictionary<Guid, IRequest<TRequest, TReply>> _requests =
            new Dictionary<Guid, IRequest<TRequest, TReply>>();
        private volatile bool _running = true;

        public AsyncRequestRemotingPort(Context context,
                                        string address,
                                        int basePort,
                                        Func<TRequest, byte[]> requestMarshaller,
                                        Func<byte[], TReply> replyUnmarshaller)
            : this(context, address, basePort, requestMarshaller, replyUnmarshaller, new PoolFiber())
        {
        }

        public AsyncRequestRemotingPort(Context context,
                                        string address,
                                        int basePort,
                                        Func<TRequest, byte[]> requestMarshaller,
                                        Func<byte[], TReply> replyUnmarshaller,
                                        IFiber fiber)
        {
            _requestMarshaller = requestMarshaller;
            _replyUnmarshaller = replyUnmarshaller;
            _fiber = fiber;
            _internalChannel.SetRequestHandler(_fiber, OnRequest);
            _replyContext = context;
            _replySocket = _replyContext.CreateSocket(SocketType.SUB);
            _replySocket.Connect(address + ":" + (basePort + 1));
            _replySocket.Subscribe(_id);
            _requestSocket = _replyContext.CreateSocket(SocketType.PUSH);
            _requestSocket.Connect(address + ":" + basePort);
            _fiber.Start();
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            return _internalChannel.SendRequest(request, fiber, onReply);
        }

        public void Dispose()
        {
            _running = false;
        }

        private static byte[] GetId()
        {
            return Guid.NewGuid().ToByteArray();
        }

        private void Run()
        {
            while (_running)
            {
                Message msg = _replySocket.ReceiveMessage(); //_fromMilliseconds);
                if (msg.IsEmpty)
                    continue;
                if (msg.FrameCount != 3)
                    throw new Exception("Msg error");
                var guid = new Guid(msg[1]);
                if (!_requests.ContainsKey(guid))
                    throw new Exception("We don't have a msg SenderId for this reply");
                TReply reply = _replyUnmarshaller(msg[2].Buffer);
                _fiber.Enqueue(() => Send(guid, reply));
            }
            InternalDispose();
        }

        private void Send(Guid guid, TReply reply)
        {
            //TODO:  add check for request.
            IRequest<TRequest, TReply> request = _requests[guid];
            request.Reply(reply);
            _requests.Remove(guid);
        }

        private void InternalDispose()
        {
            _replySocket.Dispose();
            _requestSocket.Dispose();
            _fiber.Dispose();
        }

        private void OnRequest(IRequest<TRequest, TReply> obj)
        {
            byte[] msgId = GetId();
            _requests[new Guid(msgId)] = obj;
            _requestSocket.SendMore(_id);
            _requestSocket.SendMore(msgId);
            byte[] requestData = _requestMarshaller(obj.Request);
            _requestSocket.Send(requestData);
        }
    }
}