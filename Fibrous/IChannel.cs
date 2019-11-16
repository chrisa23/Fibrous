using System;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     IChannels are in-memory conduits for messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannel<T> : IPublisherPort<T>, ISubscriberPort<T>
    {
    }


    public sealed class Channel<T> : IChannel<T>
    {
        private readonly Event<T> _internalEvent = new Event<T>();

        public void Publish(T msg)
        {
            _internalEvent.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            var disposable = _internalEvent.Subscribe(fiber.Receive(receive));
            return new Unsubscriber(disposable, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<T, Task> receive)
        {
            var disposable = _internalEvent.Subscribe(fiber.Receive(receive));
            return new Unsubscriber(disposable, fiber);
        }

        internal bool HasSubscriptions()
        {
            return _internalEvent.HasSubscriptions();
        }
    }
}