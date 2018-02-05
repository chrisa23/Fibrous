namespace Fibrous.Channels
{
    using System;

    public sealed class Channel<T> : IChannel<T>
    {
        private readonly Event<T> _internalEvent = new Event<T>();

        internal bool HasSubscriptions()
        {
            return _internalEvent.HasSubscriptions();
        }

        public void Publish(T msg)
        {
            _internalEvent.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            IDisposable disposable = _internalEvent.Subscribe(fiber.Receive(receive));
            return new Unsubscriber(disposable, fiber);
        }
    }
}