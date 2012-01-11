using System;
using Fibrous.Internal;

namespace Fibrous.Channels
{
    /// <summary>
    /// Channels are in memory conduits...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Channel<T> : IChannel<T>
    {
        private readonly EventChannel<T> _internalChannel = new EventChannel<T>();

        public bool Publish(T msg)
        {
            return _internalChannel.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            IDisposable disposable = _internalChannel.Subscribe(msg => fiber.Enqueue(() => receive(msg)));
            return new Unsubscriber(disposable, fiber);
        }
    }
}