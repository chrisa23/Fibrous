namespace Fibrous.Agents
{
    using System;

    /// <summary>
    /// Actor like abstraction.  Recieves a single type of message through a channel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChannelAgent<T> : IDisposable
    {
        protected readonly IFiber Fiber;

        public ChannelAgent(IChannel<T> channel, Action<T> handler, IExecutor executor = null)
        {
            Fiber = PoolFiber.StartNew(executor);
            channel.Subscribe(Fiber, handler);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}