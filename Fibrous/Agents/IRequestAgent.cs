namespace Fibrous
{
    using System;

    /// <summary>
    /// Actor like abstraction for request reply. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequestAgent<TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
    {
    }

    /// <summary>
    /// Agent using injected handler function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public class RequestAgent<TRequest, TReply> : IRequestAgent<TRequest, TReply>
    {
        private readonly IRequestPort<TRequest, TReply> _channel;
        protected readonly IFiber Fiber;

        public RequestAgent(Action<IRequest<TRequest, TReply>> handler, FiberType type = FiberType.Pool)
        {
            Fiber = Fibrous.Fiber.StartNew(type);
            _channel = Fiber.NewRequestPort(handler);
        }

        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            return _channel.SendRequest(request, fiber, onReply);
        }

        public IReply<TReply> SendRequest(TRequest request)
        {
            return _channel.SendRequest(request);
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

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}