namespace Fibrous
{
    using System;

    public interface ISnapshotChannel<T, TSnapshot> : IPublisherPort<T>, ISnapshotPublisherPort<TSnapshot>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot);
    }

    public interface ISnapshotPublisherPort<in T>
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T> reply);
    }

    public sealed class SnapshotChannel<T, TSnapshot> : ISnapshotChannel<T, TSnapshot>
    {
        private readonly IRequestChannel<object, TSnapshot> _requestChannel =
            new RequestChannel<object, TSnapshot>();
        private readonly IChannel<T> _updatesChannel = new Channel<T>();

        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="receive"></param>
        ///<param name="receiveSnapshot"> </param>
        public IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot)
        {
            //Action<T[]> receiveSnapshot = obj => Array.ForEach(obj, receive);
            var primedSubscribe = new SnapshotRequest<T, TSnapshot>(fiber, _updatesChannel, receive, receiveSnapshot);
            _requestChannel.SendRequest(null, fiber, x => primedSubscribe.Publish(x));
            return primedSubscribe;
        }

        public IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply)
        {
            return _requestChannel.SetRequestHandler(fiber, x => x.Reply(reply()));
        }

        public bool Publish(T msg)
        {
            return _updatesChannel.Publish(msg);
        }

        private sealed class SnapshotRequest<T, TSnapshot> : IPublisherPort<TSnapshot>, IDisposable
        {
            private readonly IFiber _fiber;
            private readonly Action<T> _receive;
            private readonly Action<TSnapshot> _receiveSnapshot;
            private readonly ISubscriberPort<T> _updatesPort;
            private bool _disposed;
            private IDisposable _sub;

            public SnapshotRequest(IFiber fiber,
                ISubscriberPort<T> updatesPort,
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