namespace Fibrous.Actors
{
    using System;
    using Fibrous.Channels;
    using Fibrous.Fibers;

    public sealed class Actor<TMsg> : Disposables, IActor<TMsg>
    {
        private readonly IFiber _fiber;
        private readonly IChannel<TMsg> _channel = new Channel<TMsg>();

        public Actor(IFiber fiber, Action<TMsg> handler)
        {
            _fiber = fiber;
            _channel.Subscribe(fiber, handler);
        }

        public Actor(Action<TMsg> handler)
            : this(new PoolFiber(), handler)
        {
        }

        public void Start()
        {
            _fiber.Start();
        }

        public void Send(TMsg msg)
        {
            _channel.Publish(msg);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fiber.Dispose();
            }
            base.Dispose(disposing);
        }

        public static IActor<TMsg> StartNew(Action<TMsg> handler)
        {
            var actor = new Actor<TMsg>(handler);
            actor.Start();
            return actor;
        }
    }
}