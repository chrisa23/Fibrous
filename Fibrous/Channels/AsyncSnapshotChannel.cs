namespace Fibrous.Channels
{
    using System;

    public class AsyncSnapshotChannel<T, TSnapshot> : ISnapshotChannel<T, TSnapshot>
    {
        private readonly IChannel<T> _updatesChannel = new Channel<T>();
        private readonly IAsyncRequestReplyChannel<object, TSnapshot> _requestChannel =
            new AsyncRequestReplyChannel<object, TSnapshot>();

        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="receive"></param>
        ///<param name="receiveSnapshot"> </param>
        public IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot)
        {
            var primedSubscribe = new SnapshotRequest(fiber, _updatesChannel, receive, receiveSnapshot);
            _requestChannel.SendRequest(null, fiber, x => primedSubscribe.Send(x));
            return primedSubscribe;
        }

        public IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply)
        {
            return _requestChannel.SetRequestHandler(fiber, x => x.Send(reply()));
        }

        public bool Send(T msg)
        {
            return _updatesChannel.Send(msg);
        }

        private class SnapshotRequest : ISenderPort<TSnapshot>, IDisposable
        {
            private readonly IFiber _fiber;
            private readonly IChannel<T> _updatesChannel;
            private readonly Action<T> _receive;
            private readonly Action<TSnapshot> _receiveSnapshot;
            private bool _disposed;
            private IDisposable _sub;

            public SnapshotRequest(IFiber fiber,
                                   IChannel<T> updatesChannel,
                                   Action<T> receive,
                                   Action<TSnapshot> receiveSnapshot)
            {
                _fiber = fiber;
                _updatesChannel = updatesChannel;
                _receive = receive;
                _receiveSnapshot = receiveSnapshot;
                _fiber.Add(this);
            }

            public bool Send(TSnapshot msg)
            {
                if (_disposed)
                {
                    return false;
                }
                _fiber.Enqueue(() => _receiveSnapshot(msg));
                //publishing the snapshot subscribes the updates...
                _sub = _updatesChannel.Subscribe(_fiber, _receive);
                return true;
            }

            public void Dispose()
            {
                _disposed = true;
                _fiber.Remove(this);
                if (_sub != null)
                {
                    _sub.Dispose();
                }
            }
        }
    }
}