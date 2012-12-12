namespace Fibrous.Experimental.Actors
{
    using System;

    public sealed class BatchingActor<TMsg> : ActorBase<TMsg>
    {
        private readonly Action<TMsg[]> _handler;
        private readonly TimeSpan _time;

        public BatchingActor(Fiber fiber, Action<TMsg[]> handler, TimeSpan time)
            : base(fiber)
        {
            _handler = handler;
            _time = time;
            HandleSubscribe(Channel);
        }

        public BatchingActor(Action<TMsg[]> handler, TimeSpan time)
            : this(new PoolFiber(), handler, time)
        {
        }

        public static BatchingActor<TMsg> Start(Action<TMsg[]> handler, TimeSpan time)
        {
            var actor = new BatchingActor<TMsg>(handler, time);
            actor.Start();
            return actor;
        }

        protected override void HandleSubscribe(ISubscriberPort<TMsg> port)
        {
            port.SubscribeToBatch(Fiber, _handler, _time);
        }
    }
}