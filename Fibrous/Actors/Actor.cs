namespace Fibrous.Actors
{
    using System;

    public sealed class Actor<TMsg> : ActorBase<TMsg>
    {
        private readonly Action<TMsg> _handler;

        public Actor(Fiber fiber, Action<TMsg> handler) : base(fiber)
        {
            _handler = handler;
            HandleSubscribe(Channel);
        }

        public Actor(Action<TMsg> handler)
            : this(new PoolFiber(), handler)
        {
        }

        public static Actor<TMsg> Start(Action<TMsg> handler)
        {
            var actor = new Actor<TMsg>(handler);
            actor.Start();
            return actor;
        }

        protected override void HandleSubscribe(ISubscriberPort<TMsg> port)
        {
            port.Subscribe(Fiber, _handler);
        }
    }
}