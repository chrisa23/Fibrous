namespace Fibrous
{
    using System;
    /// <summary>
    /// Actor like abstraction.  Recieves a single type of message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAgent<T> : IPublisherPort<T>, IDisposable
    {
    }
    
    /// <summary>
    /// Base class for a simple Agent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AgentBase<T> : IAgent<T>
    {
        private readonly IPublisherPort<T> _channel;
        protected readonly IFiber Fiber;

        protected AgentBase(FiberType type = FiberType.Pool)
        {
            Fiber = Fibrous.Fiber.StartNew(type);
            _channel = Fiber.NewPublishPort<T>(Handler);
        }

        public bool Publish(T msg)
        {
            return _channel.Publish(msg);
        }

        protected abstract void Handler(T msg);

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }

    /// <summary>
    /// Agent using injected handler function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Agent<T> : AgentBase<T>
    {
        private readonly Action<T> _handler;

        public Agent(Action<T> handler, FiberType type = FiberType.Pool) : base(type)
        {
            _handler = handler;
        }

        protected override void Handler(T msg)
        {
            _handler(msg);
        }

        /// <summary>
        /// Create and start an agent
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IAgent<T> Start(Action<T> handler, FiberType type = FiberType.Pool)
        {
            return new Agent<T>(handler, type);
        }
    }
}