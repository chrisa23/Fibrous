namespace Fibrous.Actors
{
    using System;

    public abstract class ActorBase<TMsg> : IActor<TMsg>
    {
        protected readonly IChannel<TMsg> Channel = new Channel<TMsg>();
        protected readonly Fiber Fiber;

        protected ActorBase(Fiber fiber)
        {
            Fiber = fiber;
        }

        protected ActorBase()
            : this(new PoolFiber())
        {
        }

        public void Start()
        {
            Fiber.Start();
        }

        public bool Publish(TMsg msg)
        {
            return Channel.Publish(msg);
        }

        public void Dispose()
        {
            //we want to complete pending work
            Fiber.Dispose();
        }

        public void Add(IDisposable toAdd)
        {
            Fiber.Add(toAdd);
        }

        public void Remove(IDisposable toRemove)
        {
            Fiber.Remove(toRemove);
        }

        protected abstract void HandleSubscribe(ISubscriberPort<TMsg> port);
    }
}