namespace Fibrous.Actors
{
    using Fibrous.Channels;
    using Fibrous.Fibers;

    public abstract class ActorBase<TMsg> : Disposables, IActor<TMsg>
    {
        private readonly IFiber _fiber;
        private readonly IChannel<TMsg> _channel = new Channel<TMsg>();
        protected abstract void OnMessage(TMsg msg);

        protected ActorBase(IFiber fiber)
        {
            _fiber = fiber;
            _channel.Subscribe(fiber, OnMessage);
        }

        protected ActorBase()
            : this(new PoolFiber())
        {
        }

        public void Start()
        {
            _fiber.Start();
        }

        public bool Send(TMsg msg)
        {
            return _channel.Send(msg);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fiber.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}