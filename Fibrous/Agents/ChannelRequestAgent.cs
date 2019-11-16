namespace Fibrous.Agents
{
    using System;

    /// <summary>
    /// Agent using injected handler function.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public class ChannelRequestAgent<TRequest, TReply> : IDisposable
    {
        protected readonly IFiber Fiber;

        public ChannelRequestAgent(IRequestChannel<TRequest, TReply> channel,
            Action<IRequest<TRequest, TReply>> handler, IExecutor executor = null)
        {
            Fiber = PoolFiber.StartNew(executor);
            channel.SetRequestHandler(Fiber, handler);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}