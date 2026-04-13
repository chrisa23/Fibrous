using System;
using System.Threading.Tasks;

namespace Fibrous;

public sealed class SnapshotChannel<T, TSnapshot> : ISnapshotChannel<T, TSnapshot>
{
    private readonly RequestChannel<AsyncSnapshotRequest, TSnapshot> _requestChannel = new();

    private readonly Channel<T> _updatesChannel = new();

    /// <summary>
    ///     Subscribes for an initial snapshot and then incremental update.
    /// </summary>
    /// <param name="fiber">the target executor to receive the message</param>
    /// <param name="receive"></param>
    /// <param name="receiveSnapshot"> </param>
    public IDisposable Subscribe(IFiber fiber, Func<T, Task> receive, Func<TSnapshot, Task> receiveSnapshot)
    {
        AsyncSnapshotRequest primedSubscribe = new(fiber, receive, receiveSnapshot);
        _requestChannel.SendRequest(primedSubscribe, fiber, x =>
        {
            return primedSubscribe.PublishSnapshotAsync(x);
        });
        return new Unsubscriber(primedSubscribe, fiber);
    }

    public IDisposable ReplyToPrimingRequest(IFiber fiber, Func<Task<TSnapshot>> reply) => _requestChannel.SetRequestHandler(
        fiber,
        async x =>
        {
            x.Request.SubscribeToUpdates(_updatesChannel);
            x.Reply(await reply());
        });

    public void Publish(T msg) => _updatesChannel.Publish(msg);

    public void Dispose()
    {
        _requestChannel.Dispose();
        _updatesChannel.Dispose();
    }

    private sealed class AsyncSnapshotRequest : IDisposable
    {
        private readonly IFiber _fiber;
        private readonly Func<T, Task> _receive;
        private readonly Func<TSnapshot, Task> _receiveSnapshot;
        private bool _disposed;
        private IDisposable _subscription;

        public AsyncSnapshotRequest(IFiber fiber,
            Func<T, Task> receive,
            Func<TSnapshot, Task> receiveSnapshot)
        {
            _fiber = fiber;
            _receive = receive;
            _receiveSnapshot = receiveSnapshot;
            _fiber.Add(this);
        }

        public void Dispose()
        {
            _disposed = true;
            _fiber.Remove(this);
            _subscription?.Dispose();
        }

        public Task PublishSnapshotAsync(TSnapshot msg)
        {
            if (_disposed)
            {
                return Task.CompletedTask;
            }

            return _receiveSnapshot(msg);
        }

        public void SubscribeToUpdates(ISubscriberPort<T> updatesPort)
        {
            if (_disposed)
            {
                return;
            }

            _subscription = ((IInlineSubscriberPort<T>)updatesPort).SubscribeInline(PublishUpdate);
        }

        private void PublishUpdate(T msg)
        {
            if (_disposed)
            {
                return;
            }

            _fiber.Enqueue(() => _receive(msg));
        }
    }
}
