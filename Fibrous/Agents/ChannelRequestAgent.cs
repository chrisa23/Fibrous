namespace Fibrous
{
    using System;

    /// <summary>
    /// Agent using injected handler function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        /// <summary>
        /// Create and start an agent
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDisposable Start(IRequestChannel<TRequest, TReply> channel,
            Action<IRequest<TRequest, TReply>> handler,
            FiberType type = FiberType.Pool)
        {
            return new ChannelRequestAgent<TRequest, TReply>(channel, handler, type);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}