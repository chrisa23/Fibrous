using System;

namespace Fibrous.Agents
{
    /// <summary>
    ///     Agent using injected handler function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Agent<T> : IAgent<T>
    {
        private readonly IPublisherPort<T> _channel;
        protected readonly IFiber Fiber;

        public Agent(Action<T> handler, IExecutor executor = null)
        {
            Fiber = new Fiber(executor);
            _channel = Fiber.NewChannel(handler);
        }

        public void Publish(T msg)
        {
            _channel.Publish(msg);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}