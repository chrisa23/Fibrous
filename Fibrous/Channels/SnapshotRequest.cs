namespace Fibrous.Channels
{
    using System;

    internal class SnapshotRequest<T, TSnapshot> : IPublishPort<TSnapshot>, IDisposable
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

        public bool Publish(TSnapshot msg)
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