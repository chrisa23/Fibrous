namespace Fibrous
{
    using System;

    /// <summary>
    /// Actor like abstraction for request reply. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestAgent<TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposableRegistry
    {
    }


    /// <summary>
    /// Base class for a simple Agent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RequestAgentBase<TRequest, TReply> : IRequestAgent<TRequest, TReply>
    {
        private readonly IRequestPort<TRequest, TReply> _channel;
        private readonly IFiber _fiber;

        protected RequestAgentBase(FiberType type = FiberType.Pool)
        {
            _fiber = Fiber.StartNew(type);
            _channel = _fiber.NewRequestPort<TRequest, TReply>(Handler);
        }

        protected abstract void Handler(IRequest<TRequest, TReply> request);
        
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

        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            return _channel.SendRequest(request, fiber, onReply);
        }

        public IReply<TReply> SendRequest(TRequest request)
        {
            return _channel.SendRequest(request);
        }
    }

    /// <summary>
    /// Agent using injected handler function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class RequestAgent<TRequest, TReply> : RequestAgentBase<TRequest, TReply>
    {
        private readonly Action<IRequest<TRequest, TReply>> _handler;

        public RequestAgent(Action<IRequest<TRequest, TReply>> handler, FiberType type = FiberType.Pool)
            : base(type)
        {
            _handler = handler;
        }

        /// <summary>
        /// Create and start an agent
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IRequestAgent<TRequest, TReply> Start(Action<IRequest<TRequest, TReply>> handler, FiberType type = FiberType.Pool)
        {
            return new RequestAgent<TRequest, TReply>(handler, type);
        }

        protected override void Handler(IRequest<TRequest, TReply> request)
        {
            _handler(request);
        }
    }
}