using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public sealed class SnapshotChannel<T, TSnapshot> : ISnapshotChannel<T, TSnapshot>
    {
        private readonly IRequestChannel<object, TSnapshot> _requestChannel =
            new RequestChannel<object, TSnapshot>();

        private readonly IChannel<T> _updatesChannel = new Channel<T>();

        /// <summary>
        ///     Subscribes for an initial snapshot and then incremental update.
        /// </summary>
        /// <param name="fiber">the target executor to receive the message</param>
        /// <param name="receive"></param>
        /// <param name="receiveSnapshot"> </param>
        public IDisposable Subscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot)
        {
            SnapshotRequest primedSubscribe = new SnapshotRequest(fiber, _updatesChannel, receive, receiveSnapshot);
            _requestChannel.SendRequest(null, fiber, x => primedSubscribe.Publish(x));
            return new Unsubscriber(primedSubscribe, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<T, Task> receive, Func<TSnapshot, Task> receiveSnapshot)
        {
            AsyncSnapshotRequest primedSubscribe =
                new AsyncSnapshotRequest(fiber, _updatesChannel, receive, receiveSnapshot);
            _requestChannel.SendRequest(null, fiber, x =>
            {
                primedSubscribe.Publish(x);
                return Task.CompletedTask;
            });
            return new Unsubscriber(primedSubscribe, fiber);
        }

        public IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply) =>
            _requestChannel.SetRequestHandler(fiber, x => x.Reply(reply()));

        public IDisposable ReplyToPrimingRequest(IAsyncFiber fiber, Func<Task<TSnapshot>> reply) =>
            _requestChannel.SetRequestHandler(fiber, async x => x.Reply(await reply()));

        public void Publish(T msg) => _updatesChannel.Publish(msg);

        public void Dispose()
        {
            _requestChannel.Dispose();
            _updatesChannel.Dispose();
        }

        private sealed class SnapshotRequest : IPublisherPort<TSnapshot>, IDisposable
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
                _sub?.Dispose();
            }

            public void Publish(TSnapshot msg)
            {
                if (_disposed)
                {
                    return;
                }

                _fiber.Enqueue(() => _receiveSnapshot(msg));
                //publishing the snapshot subscribes the updates...
                _sub = _updatesPort.Subscribe(_fiber, _receive);
            }
        }

        private sealed class AsyncSnapshotRequest : IPublisherPort<TSnapshot>, IDisposable
        {
            private readonly IAsyncFiber _fiber;
            private readonly Func<T, Task> _receive;
            private readonly Func<TSnapshot, Task> _receiveSnapshot;
            private readonly ISubscriberPort<T> _updatesPort;
            private bool _disposed;
            private IDisposable _sub;

            public AsyncSnapshotRequest(IAsyncFiber fiber,
                ISubscriberPort<T> updatesPort,
                Func<T, Task> receive,
                Func<TSnapshot, Task> receiveSnapshot)
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
                _sub?.Dispose();
            }

            public void Publish(TSnapshot msg)
            {
                if (_disposed)
                {
                    return;
                }

                _fiber.Enqueue(() => _receiveSnapshot(msg));
                //publishing the snapshot subscribes the updates...
                _sub = _updatesPort.Subscribe(_fiber, _receive);
            }
        }
    }
}
