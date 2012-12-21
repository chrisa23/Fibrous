namespace Fibrous.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CrossroadsIO;

    public sealed class AsyncRequestSocket<TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
    {
        private readonly Fiber _fiber;
        private readonly byte[] _id = GetId();
        private readonly IRequestChannel<TRequest, TReply> _internalChannel =
            new RequestChannel<TRequest, TReply>();
        private readonly Context _replyContext;
        private readonly Socket _replySocket;
        private readonly Func<byte[], TReply> _replyUnmarshaller;
        private readonly Func<TRequest, byte[]> _requestMarshaller;
        private readonly Socket _requestSocket;
        private readonly Dictionary<Guid, IRequest<TRequest, TReply>> _requests =
            new Dictionary<Guid, IRequest<TRequest, TReply>>();
        private volatile bool _running = true;

        public AsyncRequestSocket(Context context,
                                  string address,
                                  Func<TRequest, byte[]> requestMarshaller,
                                  Func<byte[], TReply> replyUnmarshaller)
            : this(context, address, requestMarshaller, replyUnmarshaller, new PoolFiber())
        {
        }

        public AsyncRequestSocket(Context context,
                                  string address,
                                  Func<TRequest, byte[]> requestMarshaller,
                                  Func<byte[], TReply> replyUnmarshaller,
                                  Fiber fiber)
        {
            _requestMarshaller = requestMarshaller;
            _replyUnmarshaller = replyUnmarshaller;
            _fiber = fiber;
            _internalChannel.SetRequestHandler(_fiber, OnRequest);
            _replyContext = context;
            int basePort = int.Parse(address.Split(':')[2]);
            _replySocket = _replyContext.CreateSocket(SocketType.SUB);
            _replySocket.Connect(address.Substring(0, address.LastIndexOf(":")) + ":" + (basePort + 1));
            _replySocket.Subscribe(_id);
            _requestSocket = _replyContext.CreateSocket(SocketType.PUSH);
            _requestSocket.Connect(address);
            _fiber.Start();
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public IDisposable SendRequest(TRequest request, Fiber fiber, Action<TReply> onReply)
        {
            return _internalChannel.SendRequest(request, fiber, onReply);
        }

        public IReply<TReply> SendRequest(TRequest request)
        {
            return _internalChannel.SendRequest(request);
        }

        public void Dispose()
        {
            _replySocket.Close();
        }

        private static byte[] GetId()
        {
            return Guid.NewGuid().ToByteArray();
        }

        private void Run()
        {
            while (_running)
            {
                try
                {
                    Message msg = _replySocket.ReceiveMessage();
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
                catch (Exception e)
                {
                    _running = false;
                }
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