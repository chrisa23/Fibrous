namespace Fibrous.Channels
{
    using System;

    public sealed class AsyncSnapshotChannel<T, TSnapshot> : IAsyncSnapshotChannel<T, TSnapshot>
    {
        private readonly IAsyncRequestChannel<object, TSnapshot> _requestChannel =
            new AsyncRequestChannel<object, TSnapshot>();
        private readonly IChannel<T> _updatesChannel = new Channel<T>();

        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="receive"></param>
        ///<param name="receiveSnapshot"> </param>
        public IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive, Action<TSnapshot> receiveSnapshot)
        {
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
    }
}