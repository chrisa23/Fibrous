namespace Fibrous.Agents
{
    using System;

    /// <summary>
    /// Agent using injected handler function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Agent<T> : IAgent<T>
    {
        private readonly IPublisherPort<T> _channel;
        protected readonly IFiber Fiber;

        public Agent(Action<T> handler, IExecutor executor = null)
        {
            Fiber = PoolFiber.StartNew(executor);
            _channel = Fiber.NewPublishPort(handler);
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