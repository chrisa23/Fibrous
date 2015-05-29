namespace Fibrous
{
    using System;

    /// <summary>
    /// Channels are in-memory conduits for messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannel<T> : IPublisherPort<T>, ISubscriberPort<T>
    {
    }

    public sealed class Channel<T> : IChannel<T>
    {
        private readonly Event<T> _internalChannel = new Event<T>();

        public bool Publish(T msg)
        {
            return _internalChannel.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            IDisposable disposable = _internalChannel.Subscribe(Receive(fiber, receive));
            return new Unsubscriber(disposable, fiber);
        }

        private static Action<T> Receive(IExecutionContext fiber, Action<T> receive)
        {
            return msg => fiber.Enqueue(() => receive(msg));
        }
    }
}