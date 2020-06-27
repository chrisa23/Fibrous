using System;
using System.Threading.Tasks;

namespace Fibrous.Agents
{
    /// <summary>
    ///     Agent using injected handler function.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public class RequestAgent<TRequest, TReply> :IRequestAgent<TRequest, TReply>
    {
        private readonly IRequestPort<TRequest, TReply> _channel;
        protected IFiber Fiber;

        public RequestAgent(Action<IRequest<TRequest, TReply>> handler, Action<Exception> errorHandler = null)
        {
            Fiber = errorHandler == null ? new Fiber() : new Fiber(errorHandler);
            _channel = Fiber.NewRequestPort(handler);
        }
        public RequestAgent(IFiberFactory factory, Action<IRequest<TRequest, TReply>> handler, Action<Exception> errorHandler = null)
        {
            Fiber = factory.Create(errorHandler);
            _channel = Fiber.NewRequestPort(handler);
        }
        public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply)
        {
            return _channel.SendRequest(request, fiber, onReply);
        }

        public IDisposable SendRequest(TRequest request, IAsyncFiber fiber, Func<TReply, Task> onReply)
        {
            return _channel.SendRequest(request, fiber, onReply);
        }

        public Task<TReply> SendRequest(TRequest request)
        {
            return _channel.SendRequest(request);
        }
        
        public Task<Result<TReply>> SendRequest(TRequest request, TimeSpan timeout)
        {
            return _channel.SendRequest(request, timeout);
        }

        public void Dispose()
        {
            Fiber?.Dispose();
        }
    }
}