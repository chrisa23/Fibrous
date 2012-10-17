namespace Fibrous.Channels
{
    using System;

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
        ///<param name="timeout"> </param>
        public IDisposable PrimedSubscribe(IFiber fiber,
                                           Action<T> receive,
                                           Action<TSnapshot> receiveSnapshot,
                                           TimeSpan timeout)
        {
            var primedSubscribe = new SnapshotRequest<T, TSnapshot>(fiber, _updatesChannel, receive, receiveSnapshot);
            TSnapshot reply = _requestChannel.SendRequest(null, timeout);
            receiveSnapshot(reply);
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