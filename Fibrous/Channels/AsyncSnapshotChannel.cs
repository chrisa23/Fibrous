using System;

namespace Fibrous.Channels
{
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
        ///<param name="snapshotReceive"> </param>
        public IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> snapshotReceive)
        {
            var primedSubscribe = new SnapshotRequest(fiber, _updatesChannel, receive, snapshotReceive);
            _requestChannel.SendRequest(null, fiber, x => primedSubscribe.Publish(x));
            return primedSubscribe;
        }

        public IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply)
        {
            return _requestChannel.SetRequestHandler(fiber, x => x.Publish(reply()));
        }

        public bool Publish(T msg)
        {
            return _updatesChannel.Publish(msg);
        }

        private class SnapshotRequest : IPublisherPort<TSnapshot>, IDisposable
        {
            private readonly IFiber _fiber;
            private readonly IChannel<T> _updatesChannel;
            private readonly Action<T> _receive;
            private readonly Action<TSnapshot> _snapshotReceive;
            private bool _disposed;
            private IDisposable _sub;

            public SnapshotRequest(IFiber fiber, IChannel<T> updatesChannel, Action<T> receive,
                                   Action<TSnapshot> snapshotReceive)
            {
                _fiber = fiber;
                _updatesChannel = updatesChannel;
                _receive = receive;
                _snapshotReceive = snapshotReceive;
                _fiber.Add(this);
            }

            public bool Publish(TSnapshot msg)
            {
                if (_disposed) return false;

                _fiber.Enqueue(() => _snapshotReceive(msg));

                //publishing the snapshot subscribes the updates...
                _sub = _updatesChannel.Subscribe(_fiber, _receive);

                return true;
            }

            public void Dispose()
            {
                _disposed = true;
                _fiber.Remove(this);
                if (_sub != null)
                    _sub.Dispose();
            }
        }
    }
}