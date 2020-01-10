using System;
using System.Threading.Tasks;

namespace Fibrous.Agents
{
    /// <summary>
    ///     Agent using injected handler function.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public class AsyncRequestAgent<TRequest, TReply> : IRequestAgent<TRequest, TReply>
    {
        private readonly IRequestPort<TRequest, TReply> _channel;
        protected readonly IAsyncFiber Fiber;

        public AsyncRequestAgent(Func<IRequest<TRequest, TReply>,Task> handler, IAsyncExecutor executor = null)
        {
            Fiber = new AsyncFiber(executor);
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

        public void Dispose()
        {
            Fiber.Dispose();
        }

        /// <summary>
        ///     Create and start an agent
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IRequestAgent<TRequest, TReply> Start(Action<IRequest<TRequest, TReply>> handler)
        {
            return new RequestAgent<TRequest, TReply>(handler);
        }
    }
}