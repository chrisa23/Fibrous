namespace Fibrous.Agents
{
    using System;
    using Fibrous.Channels;

    /// <summary>
    /// Agent using injected handler function.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public class ChannelRequestAgent<TRequest, TReply> : IDisposable
    {
        protected readonly IFiber Fiber;

        public ChannelRequestAgent(IRequestChannel<TRequest, TReply> channel,
            Action<IRequest<TRequest, TReply>> handler,
            FiberType type = FiberType.Pool)
        {
            Fiber = Fibrous.Fiber.StartNew(type);
            channel.SetRequestHandler(Fiber, handler);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}