namespace Fibrous
{
    using System;

    /// <summary>
    /// Actor like abstraction.  Recieves a single type of message through a channel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChannelAgent<T> : IDisposable
    {
        protected readonly IFiber Fiber;

        public ChannelAgent(IChannel<T> channel, Action<T> handler, FiberType type = FiberType.Pool)
        {
            Fiber = Fibrous.Fiber.StartNew(type);
            channel.Subscribe(Fiber, handler);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}