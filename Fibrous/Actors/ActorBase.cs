namespace Fibrous.Actors
{
    using System;
    using Fibrous.Channels;
    using Fibrous.Fibers;

    public abstract class ActorBase<TMsg> : IActor<TMsg>
    {
        protected readonly IFiber Fiber;
        private readonly IChannel<TMsg> _channel = new Channel<TMsg>();

        protected ActorBase(IFiber fiber)
        {
            Fiber = fiber;
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            HandleSubscribe(_channel);
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        //All classes inheriting from this class must override this and should be sealed.
        protected abstract void HandleSubscribe(ISubscribePort<TMsg> port);

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
            return _channel.Publish(msg);
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
    }
}