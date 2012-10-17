namespace Fibrous.Channels
{
    using System;
    using Fibrous.Utility;

    /// <summary>
    /// Channels are in memory conduits...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Channel<T> : IChannel<T>
    {
        private readonly EventWrapper<T> _internalChannel = new EventWrapper<T>();

        public bool Publish(T msg)
        {
            return _internalChannel.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            IDisposable disposable = _internalChannel.Subscribe(Receive(fiber, receive));
            return new Unsubscriber(disposable, fiber);
        }

        private static Action<T> Receive(IFiber fiber, Action<T> receive)
        {
            return msg => fiber.Enqueue(() => receive(msg));
        }
    }
}