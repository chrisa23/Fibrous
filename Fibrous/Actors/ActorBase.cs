using System;
using Fibrous.Channels;
using Fibrous.Fibers;

namespace Fibrous.Actors
{
    public abstract class ActorBase<TMsg> : IActor<TMsg>
    {
        protected readonly IChannel<TMsg> Channel = new Channel<TMsg>();
        protected readonly IFiber Fiber;

        protected ActorBase(IFiber fiber)
        {
            Fiber = fiber;
        }

        protected ActorBase()
            : this(new PoolFiber())
        {
        }

        #region IActor<TMsg> Members

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

        #endregion

        protected abstract void HandleSubscribe(ISubscribePort<TMsg> port);
    }
}