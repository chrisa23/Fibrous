using System;

namespace Fibrous.Channels
{
    internal sealed class SnapshotRequest<T, TSnapshot> : IPublishPort<TSnapshot>, IDisposable
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

        #region IDisposable Members

        public void Dispose()
        {
            _disposed = true;
            _fiber.Remove(this);
            if (_sub != null)
                _sub.Dispose();
        }

        #endregion

        #region IPublishPort<TSnapshot> Members

        public bool Publish(TSnapshot msg)
        {
            if (_disposed)
                return false;
            _fiber.Enqueue(() => _receiveSnapshot(msg));
            //publishing the snapshot subscribes the updates...
            _sub = _updatesPort.Subscribe(_fiber, _receive);
            return true;
        }

        #endregion
    }
}