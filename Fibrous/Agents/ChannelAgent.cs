using System;

namespace Fibrous.Agents
{
    /// <summary>
    ///     Actor like abstraction.  Receives a single type of message through a channel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChannelAgent<T> : IDisposable
    {
        protected readonly IFiber Fiber;

        public ChannelAgent(IChannel<T> channel, Action<T> handler, IExecutor executor = null)
        {
            Fiber = new Fiber(executor);
            channel.Subscribe(Fiber, handler);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}