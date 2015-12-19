namespace Fibrous
{
    using System;

    /// <summary>
    /// Actor like abstraction.  Recieves a single type of message directly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAgent<T> : IPublisherPort<T>, IDisposable
    {
    }

    /// <summary>
    /// Agent using injected handler function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Agent<T> : IAgent<T>
    {
        private readonly IPublisherPort<T> _channel;
        protected readonly IFiber Fiber;

        public Agent(Action<T> handler, FiberType type = FiberType.Pool)
        {
            Fiber = Fibrous.Fiber.StartNew(type);
            _channel = Fiber.NewPublishPort(handler);
        }

        public bool Publish(T msg)
        {
            return _channel.Publish(msg);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}