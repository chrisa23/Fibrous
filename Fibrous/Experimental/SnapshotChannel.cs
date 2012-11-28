namespace Fibrous.Experimental
{
    using System;
    using Fibrous.Channels;

    public sealed class SnapshotChannel<T> : ISnapshotChannel<T>
    {
        private readonly IRequestChannel<object, T[]> _requestChannel =
            new RequestChannel<object, T[]>();
        private readonly IChannel<T> _updatesChannel = new Channel<T>();

        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="receive"></param>
        ///<param name="receiveSnapshot"> </param>
        public IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<T[]> receiveSnapshot)
        {
            var primedSubscribe = new SnapshotRequest<T, T[]>(fiber, _updatesChannel, receive, receiveSnapshot);
            _requestChannel.SendRequest(null, fiber, x => primedSubscribe.Publish(x));
            return primedSubscribe;
        }

        public IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T[]> reply)
        {
            return _requestChannel.SetRequestHandler(fiber, x => x.Reply(reply()));
        }

        public bool Publish(T msg)
        {
            return _updatesChannel.Publish(msg);
        }

        private sealed class SnapshotRequest<T, TSnapshot> : IPublishPort<TSnapshot>, IDisposable
        {
            private readonly IFiber _fiber;
            private readonly Action<T> _receive;
            private readonly Action<TSnapshot> _receiveSnapshot;
            private readonly ISubscribePort<T> _updatesPort;
            private bool _disposed;
            private IDisposable _sub;

            public SnapshotRequest(IFiber fiber,
                                   ISubscribePort<T> updatesPort,
                                   Action<T> receive,
                                   Action<TSnapshot> receiveSnapshot)
            {
                _fiber = fiber;
                _updatesPort = updatesPort;
                _receive = receive;
                _receiveSnapshot = receiveSnapshot;
                _fiber.Add(this);
            }

            public void Dispose()
            {
                _disposed = true;
                _fiber.Remove(this);
                if (_sub != null)
                    _sub.Dispose();
            }

            public bool Publish(TSnapshot msg)
            {
                if (_disposed)
                    return false;
                _fiber.Enqueue(() => _receiveSnapshot(msg));
                //publishing the snapshot subscribes the updates...
                _sub = _updatesPort.Subscribe(_fiber, _receive);
                return true;
            }
        }
    }
}