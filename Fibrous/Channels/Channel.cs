namespace Fibrous.Channels
{
    using System;
    using Fibrous.Util;

    public sealed class Channel<T> : IChannel<T>
    {
        private readonly Event<T> _internalChannel = new Event<T>();

        public void Publish(T msg)
        {
            _internalChannel.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            IDisposable disposable = _internalChannel.Subscribe(fiber.Receive(receive));
            return new Unsubscriber(disposable, fiber);
        }
    }
}