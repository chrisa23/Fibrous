namespace Fibrous.Actors
{
    using System;
    using Fibrous.Channels;
    using Fibrous.Fibers;

    public sealed class Actor<TMsg> : IActor<TMsg>
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

        public bool Publish(TMsg msg)
        {
            return _channel.Publish(msg);
        }

        public static IActor<TMsg> StartNew(Action<TMsg> handler)
        {
            var actor = new Actor<TMsg>(handler);
            actor.Start();
            return actor;
        }

        public void Dispose()
        {
            _fiber.Dispose();
        }

        public void Add(IDisposable toAdd)
        {
            _fiber.Add(toAdd);
        }

        public void Remove(IDisposable toRemove)
        {
            _fiber.Remove(toRemove);
        }
    }
}